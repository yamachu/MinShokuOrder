OUTDIR = dist
OUT = $(OUTDIR)/index.js

build: init
	@npx tsc --project tsconfig.json

run: $(OUT)
	@node $<

init:
	@mkdir -p $(OUTDIR)
