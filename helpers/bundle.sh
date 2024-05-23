WWWROOT=$1

cp "src/game.js" "src/game.js.bak"

if [ -n "$SINGLEFILE" ]; then
  dotnetjs="$WWWROOT/_framework/dotnet.js"
  dotnet=$(<"$dotnetjs")

  dotnet=${dotnet//o=import\(e.resolvedUrl\)/o=window.importfill\(e.resolvedUrl\)}
  dotnet=${dotnet//n=import\(t.resolvedUrl\)/n=window.importfill\(t.resolvedUrl\)}
  dotnet=${dotnet//import.meta.url/window.location.href}

  dotnet64=$(base64 -w0 <(echo -n "$dotnet"))

  echo "import { dotnet } from 'data:text/javascript;base64,$dotnet64';" > "src/game.js"
else
  :> "src/game.js"
fi

cat "src/game.js.bak" >> "src/game.js"

cd src || exit
npx esbuild --bundle "../src/main.js" --format=esm --outfile="../$WWWROOT/bundle.js.tmp"
cd ..

mv "src/game.js.bak" "src/game.js"


if [ -n "$SINGLEFILE" ]; then
  TARGET="$WWWROOT/bundlesingle.js"
else
  TARGET="$WWWROOT/bundle.js"
fi

:> "$TARGET"

if [ -z "$SINGLEFILE" ]; then
  echo "import { dotnet } from './_framework/dotnet.js';" > "$TARGET"
fi

cat "$WWWROOT/bundle.js.tmp" >> "$TARGET"


