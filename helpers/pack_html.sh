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

  base64=$(base64 -w 0 "$WWWROOT/$file")

  parsed+="data:$mime;base64,$base64"

done

parsed+=$rest





file=compiled.html

beforemap=${parsed%%SEDHERE_IMPORTS*}
aftermap=${parsed#*SEDHERE_IMPORTS}

echo "$beforemap" > "$file"

echo "BASE64: wasm.pak"

if [ -z "$SKIPPAK" ]; then
  base64 -w0 wasm.pak >> "$file"
fi

beforeassets=${aftermap%%SEDHERE_GAME_DATA*}
afterassets=${aftermap#*SEDHERE_GAME_DATA}

echo "$beforeassets" >> "$file"

echo "BASE64: data.data"

if [ -z "$SKIPDATA" ]; then
  base64 -w0 $WWWROOT/_framework/data.data >> "$file"
fi

echo "$afterassets" >> "$file"
