CONTENTROOT=Content
RESTORE=Patches/Restore/Content

apply() {
  if [ -d "$RESTORE" ]; then
    echo "Error: need to restore before patching again"
    return
  fi
  mkdir -p "$RESTORE"

  find Patches/Content -type f -name "*.patch" | while read patch; do
    local patchfile=${patch#Patches/Content/}
    local file=${patchfile%.patch}

    if [[ $patchfile == *disabled* ]]; then
      echo "Skipping patchfile $file"
      continue
    fi

    echo "$patchfile | $file"
    mkdir -p "$RESTORE/$(dirname "$file")"
    cp "$CONTENTROOT/$file" "$RESTORE/$file"

    patch -p1 -i "$patch" "$CONTENTROOT/$file"
  done

  find Patches/Content -type f -not -name "*.patch" | while read file; do
    local file=${file#Patches/Content/}

    if [[ $file == *disabled* ]]; then
      echo "Skipping $file"
      continue
    fi

    if [[ $file == *FMOD* ]] && [ -z "$STRIPFMOD" ]; then
      echo "FMOD: Skipping $file"
      continue
    fi

    echo "$file"
    mkdir -p "$RESTORE/$(dirname "$file")"
    cp "$CONTENTROOT/$file" "$RESTORE/$file"

    cp "Patches/Content/$file" "$CONTENTROOT/$file"
  done
}

restore() {
  if [ ! -d "$RESTORE" ]; then
    echo "Error: no restore found"
    return
  fi

  cp -r "$RESTORE"/* "$CONTENTROOT"

  rm -rf "$RESTORE"
}

extract() {
  # ilspycmd: 9.0.0.7625
  # ICSharpCode.Decompiler: 9.0.0.7625

  ilspycmd $1 -p -o Decompiled
}

genpatches() {
  decompdir=../decomp/Celeste/
  find "$decompdir" -type f -name "*.cs" | while read file; do
    echo $file
    local file=${file#$decompdir}
    local patchfile=Patches/Code/Celeste/$file.patch

    mkdir -p $(dirname $patchfile)
    diff=$(diff -u ../decomp/Celeste/$file celeste/Celeste/$file)
    if [ -n "$diff" ]; then
      echo -n "$diff" > $patchfile
    fi
  done
}

$@
