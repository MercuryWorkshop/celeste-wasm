patch:
	(. ./lib.sh; _patch)

restore:
	(. ./lib.sh; restore)

pack_html:
	(. ./lib.sh; pack_html)
pack_assets:
	(. ./lib.sh; pack_assets)

debug: FORCE
	dotnet run -v d


FORCE:

