. helpers/lib.sh

WWWROOT=$1


dotnetjs="$WWWROOT/_framework/dotnet.js"
dotnet=$(<"$dotnetjs")

dotnet=${dotnet//o=import\(e.resolvedUrl\)/o=window.importfill\(e.resolvedUrl\)}
dotnet=${dotnet//n=import\(t.resolvedUrl\)/n=window.importfill\(t.resolvedUrl\)}
# dotnet=${dotnet//import.meta.url/window.location.href}

dotnet64=$(base64 -w0 <(echo -n "$dotnet"))

cp "$WWWROOT/game.js" "$WWWROOT/game.js.bak"
cp "$WWWROOT/game.js" "$WWWROOT/game.js.tmp"
sed -i '1d' "$WWWROOT/game.js.tmp"

echo "import { dotnet } from 'data:text/javascript;base64,$dotnet64';" > "$WWWROOT/game.js"
cat "$WWWROOT/game.js.tmp" >> "$WWWROOT/game.js"

cd jslibs && npx esbuild --bundle "../$WWWROOT/main.js" --format=esm --platform=node --outfile="../$WWWROOT/mainbundled.js" && cd ..
mv "$WWWROOT/game.js.bak" "$WWWROOT/game.js"
rm "$WWWROOT/game.js.tmp"



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

base64 -w0 wasm.pak >> "$file"

beforeassets=${aftermap%%SEDHERE_GAME_DATA*}
afterassets=${aftermap#*SEDHERE_GAME_DATA}

echo "$beforeassets" >> "$file"

echo "BASE64: data.data"

base64 -w0 $WWWROOT/_framework/data.data >> "$file"

echo "$afterassets" >> "$file"
