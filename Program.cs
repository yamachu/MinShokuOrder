using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using PuppeteerSharp;

namespace MinShokuOrder
{
    class Option
    {
        [Option("email", Required = true, HelpText = "ログインEmail")]
        public string Email { get; set; }

        [Option("password", Required = true, HelpText = "ログインPassword")]
        public string Password { get; set; }

        [Option('v', Default = false)]
        public bool Verbose { get; set; }
    }

    class Program
    {
        async static Task Main(string[] args)
        {
            var opt = Parser.Default.ParseArguments<Option>(args);
            if (opt.Tag == ParserResultType.NotParsed) {
                throw new Exception("引数のパースに失敗");
            }
            var parsedOpt = (Parsed<Option>)opt;

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
            
                Args = new []{"lang=ja,en-US,en"}
            });
            
            var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions{
                Width = 1200,
                Height = 800
            });

            await page.GoToAsync(Consts.URL.LOGIN, new NavigationOptions{
                WaitUntil = new []{WaitUntilNavigation.Networkidle0}
            });
            await page.TypeAsync(Consts.Query.LOGIN_EMAIL, parsedOpt.Value.Email);
            await page.TypeAsync(Consts.Query.LOGIN_PASSWORD, parsedOpt.Value.Password);
            await page.ClickAsync(Consts.Query.LOGIN_BUTTON);
            await page.WaitForNavigationAsync(new NavigationOptions{
                WaitUntil = new []{WaitUntilNavigation.Networkidle0}
            });

            await page.GoToAsync(Consts.URL.MENU_LIST, new NavigationOptions{
                WaitUntil = new []{WaitUntilNavigation.Networkidle0}
            });

            var notCancelableButtonExist = (await page.XPathAsync(Consts.Query.LIST_ORDER_DEADLINE_BUTTON))
                .Length != 0;
            var dates = await Task.WhenAll((await page.XPathAsync(Consts.Query.LIST_DATES))
                .Select(async v => await page.EvaluateFunctionAsync("elm => elm.innerText", new []{v}))
                .Select(async v => (await v).ToString().Trim()));
            var menus = await Task.WhenAll((await page.XPathAsync(Consts.Query.LIST_MENU_NAMES))
                .Select(async v => await page.EvaluateFunctionAsync("elm => elm.innerText", new []{v}))
                .Select(async v => (await v).ToString()));
            var ordered = await Task.WhenAll((await page.XPathAsync(Consts.Query.LIST_ORDER_BUTTONS))
                .Select(async v => await page.EvaluateFunctionAsync("elm => elm.className", new []{v}))
                .Select(async v => (await v).ToString().Contains("menu__btn--cancel")));

            var menuDetails = ordered.Select((o, i) => new {
                Date = dates[i + (notCancelableButtonExist ? 1 : 0)],
                Menu = menus[i + (notCancelableButtonExist ? 1 : 0)],
                Ordered = o
            }).ToArray();

            if (parsedOpt.Value.Verbose) foreach (var v in menuDetails) System.Console.WriteLine(v);

            var willOrderIndex = ordered.Select((v, i) => new { v, i }).Where(v => !v.v).Select(v => v.i);

            foreach (var i in willOrderIndex) {
                await Task.WhenAll(
                    (await page.XPathAsync(String.Format(Consts.Query.LIST_ORDER_BUTTON, i + 1)))[0].ClickAsync(),
                    page.WaitForResponseAsync(Consts.URL.CONFIRM)
                );

                await page.WaitForXPathAsync(Consts.Query.DETAIL_ORDER_BUTTON);

                await Task.WhenAll(
                    (await page.XPathAsync(Consts.Query.DETAIL_ORDER_BUTTON))[0].ClickAsync(),
                    page.WaitForResponseAsync((res) => res.Url.Contains(Consts.URL.ORDER_SUCCESS))
                );

                await page.GoToAsync(Consts.URL.MENU_LIST, new NavigationOptions{
                    WaitUntil = new []{WaitUntilNavigation.Networkidle0}
                });
            }

            await browser.CloseAsync();
        }
    }

    namespace Consts
    {
        static class URL {
            public static readonly string LOGIN = "https://minnano.shokudou.jp/users/sign_in";
            public static readonly string MENU_LIST = "https://minnano.shokudou.jp/daily_menus";
            public static readonly string CONFIRM = "https://minnano.shokudou.jp/order/confirm";
            public static readonly string ORDER_SUCCESS = "https://minnano.shokudou.jp/order/thanks/";
        }

        static class Query {
            public static readonly string LOGIN_EMAIL = "#user_email";
            public static readonly string LOGIN_PASSWORD = "#user_password";
            public static readonly string LOGIN_BUTTON = "#new_user > div > ul > li > input";

            public static readonly string LIST_DATES = "//*[@id=\"top\"]/div/main/div/ul[last()]/li[*]/section/h2/span";
            public static readonly string LIST_MENU_NAMES =
                "//*[@id=\"top\"]/div/main/div/ul[last()]/li[*]/section/ul/li[1]/section/div/h3";
            public static readonly string LIST_ORDER_BUTTONS =
                "//*[@id=\"top\"]/div/main/div/ul[last()]/li[*]/section/ul/li[1]/section/div/form/input[@name=\"commit\"]";
            public static readonly string LIST_ORDER_BUTTON =
                "//*[@id=\"top\"]/div/main/div/ul[last()]/li[{0}]/section/ul/li[1]/section/div/form/input[@name=\"commit\"]";
            public static readonly string LIST_ORDER_DEADLINE_BUTTON = "//*[@id=\"top\"]/div/main/div/ul[last()]/li[*]/section/ul/li[1]/section/div/button";

            public static readonly string DETAIL_ORDER_BUTTON =
                "//*[@id=\"top\"]/div/main/div[1]/section/form/input[@name=\"commit\"]";
        }
    }
}
