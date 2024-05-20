. helpers/lib.sh

WWWROOT=$1

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
      dotnet=${dotnet//n=import\(t.resolvedUrl\)/n=window.importfill\(t.resolvedUrl\)}
      dotnet=${dotnet//import.meta.url/window.location.href}

      dotnet64=$(echo -n "$dotnet" | base64 -w 0)

      dotnet64="data:text/javascript;base64,"$dotnet64

      content=$(<"$WWWROOT/$file")
      content=${content/.\/_framework\/dotnet.js/$dotnet64}

      echo "BASE64: dotnet.js"
      base64=$(echo -n "$content" | base64 -w 0)
      # o=import(e.resolvedUrl)
    elif [ $file == "data.js" ]; then
      content=$(<"$WWWROOT/$file")
      content=${content/packageName, t/assetblob, t}
      base64=$(echo -n "$content" | base64 -w 0)
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

  echo "BASE64: wasm.pak"

  base64 -w0 wasm.pak >> "$file"

  beforeassets=${aftermap%%SEDHERE_GAME_DATA*}
  afterassets=${aftermap#*SEDHERE_GAME_DATA}

  echo "$beforeassets" >> "$file"

  echo "BASE64: data.data"

  base64 -w0 $WWWROOT/_framework/data.data >> "$file"

  echo "$afterassets" >> "$file"
