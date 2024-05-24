WWWROOT=$1

cd src || exit
npx esbuild --jsx-factory=h --bundle "../src/main.jsx" --format=esm --outfile="../$WWWROOT/bundle.js.tmp"
cd ..


if [ -n "$SINGLEFILE" ]; then
  TARGET="$WWWROOT/bundlesingle.js"
else
  TARGET="$WWWROOT/bundle.js"
fi

if [ -n "$SINGLEFILE" ]; then
  dotnetjs="$WWWROOT/_framework/dotnet.js"
  dotnet=$(<"$dotnetjs")

  dotnet=${dotnet//o=import\(e.resolvedUrl\)/o=window.importfill\(e.resolvedUrl\)}
  dotnet=${dotnet//n=import\(t.resolvedUrl\)/n=window.importfill\(t.resolvedUrl\)}
  dotnet=${dotnet//import.meta.url/window.location.href}

  dotnet64=$(base64 -w0 <(echo -n "$dotnet"))

  echo "import { dotnet } from 'data:text/javascript;base64,$dotnet64';" > "$TARGET"
else
  echo "import { dotnet } from './_framework/dotnet.js';" > "$TARGET"
fi

cat "$WWWROOT/bundle.js.tmp" >> "$TARGET"


