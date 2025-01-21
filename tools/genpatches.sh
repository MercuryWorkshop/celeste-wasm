#!/usr/bin/env bash
set -euo pipefail
shopt -s inherit_errexit

which diff &> /dev/null || {
	echo "Please install diff."
	exit 1
}

if ! [[ -d "celeste/Decompiled" ]]; then
	echo "Please run tools/decompile.sh first"
fi

if ! [[ "$#" -eq "1" ]]; then
	echo "usage: bash tools/genpatches.sh <dir>"
	exit 1
fi

rm -r "celeste/Patches/$1" || true

DECOMPDIR="celeste/Decompiled/"
find "$DECOMPDIR"{Celeste,Celeste.Editor,Celeste.Pico8,Monocle,FMOD,FMOD.Studio} -type f -name "*.cs" | while read -r file; do
	file="${file#${DECOMPDIR}}"
	patch="celeste/Patches/$1/$file.patch"


	mkdir -p "$(dirname "$patch")"
	diff=$(diff -u --label "$DECOMPDIR$file" --label "celeste/$file" "$DECOMPDIR$file" "celeste/$file" || true)

	if [ -n "$diff" ]; then
		echo "writing diff for $file"
		echo "$diff" > "$patch"
	fi
done
