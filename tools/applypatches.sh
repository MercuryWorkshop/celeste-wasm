#!/usr/bin/env bash
set -euo pipefail
shopt -s inherit_errexit

which patch &> /dev/null || {
	echo "Please install patch."
	exit 1
}

if ! [[ -d "celeste/Patches" ]]; then
	echo "Please run tools/genpatches.sh first."
fi

find "celeste/Patches/Code" -type f -name "*.patch" | while read -r patch; do
	file=${patch#celeste/Patches/Code/}
	file=${file%.patch}

	echo "$patch | $file"
	patch --version-control=none --no-backup-if-mismatch -p1 -i "$patch" "celeste/$file"
done
