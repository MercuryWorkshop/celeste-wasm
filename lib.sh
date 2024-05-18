CONTENTROOT=wwwroot/assets/Content
RESTORE=Patches/Restore/Content

_patch() {
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



WWWROOT=bin/Debug/net8.0/wwwroot
pack_html() {
  cp -r wwwroot/main.js "$WWWROOT"
  # cp -r wwwroot/* "$WWWROOT"

  parsed=""
  rest=$(<stub.html)

  searchstring="BASE64LOAD"



  while [[ "$rest" == *BASE64LOAD* ]]; do
    parsed=${parsed}${rest%%$searchstring*}
    rest=${rest#*$searchstring}

    # cut off leading :
    rest=${rest:1}

    mime=${rest%%:*}
    rest=${rest#*:}
    file=${rest%%:*}
    rest=${rest#*:}

    echo "MIME: $mime FILE: $file"

    if [ $file == "main.js" ]; then
      dotnet=$(<"$WWWROOT/_framework/dotnet.js")


      dotnet=${dotnet//o=import\(e.resolvedUrl\)/o=window.importfill\(e.resolvedUrl\)}
      dotnet=${dotnet//import.meta.url/window.location.href}

      dotnet64=$(echo -n "$dotnet" | base64 -w 0)

      dotnet64="data:text/javascript;base64,"$dotnet64

      content=$(<"$WWWROOT/$file")
      content=${content/.\/_framework\/dotnet.js/$dotnet64}

      base64=$(echo -n "$content" | base64 -w 0)
      # o=import(e.resolvedUrl)
    else
      base64=$(base64 -w 0 "$WWWROOT/$file")
    fi

    parsed+="data:$mime;base64,$base64"

  done

  parsed+=$rest





  file=compiled.html

  beforemap=${parsed%%SEDHERE_IMPORTS*}
  aftermap=${parsed#*SEDHERE_IMPORTS}


  echo "$beforemap" > "$file"



  file=assets.json
  echo > "$file"

  while read bfile; do
    echo "Baking $bfile"
    # if [[ $bfile == *csv* ]] || [[ $bfile == *json* ]]; then
      bfile=${bfile#$WWWROOT/}

      bfilename=${bfile}

      if [[ $bfile == *_framework* ]]; then
        bfilename=${bfile#_framework/}
      fi

      echo -n "$bfilename " >> "$file"
      base64 -w 0 "$WWWROOT/$bfile" >> "$file"
      echo >> "$file"
    # fi
  done <<< "$(find "$WWWROOT" -type f)"

  file=compiled.html

  echo "$aftermap" >> "$file"

}
