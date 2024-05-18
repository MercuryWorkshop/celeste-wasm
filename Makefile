patch:
	(. ./lib.sh; _patch)

restore:
	(. ./lib.sh; restore)

pack_html:
	(. ./lib.sh; pack_html)

debug: FORCE
	dotnet run -v d


FORCE:

