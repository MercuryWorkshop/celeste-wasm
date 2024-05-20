
. helpers/lib.sh
WWWROOT=$1

file=wasm.pak
echo -n > "$file"

while read bfile; do
  bfile=${bfile#$WWWROOT/_framework/}

  bfilename=${bfile}


  if [[ $bfile =~ dotnet\.native.*\.js$  ]]; then
    contents=$(<"$WWWROOT/_framework/$bfile")
    bfile=../../../../../../../../../../../../../../../../../../$(mktemp)

    contents=${contents//new URL\(\'dotnet.native.wasm\'\, import.meta.url\).href/\'dotnet.native.wasm\'}

    echo -n "$contents" > "$bfile"
  fi

  if ! [[ $bfile =~ (.gz|.data|.pdb|.map)$ ]]; then
    echo "Baking $bfilename"
    {
      toint "$(echo -n "$bfilename" | wc -c)" | fromhex
      echo -n "$bfilename"
      toint "$(stat -c %s "$WWWROOT/_framework/$bfile")" | fromhex
      cat "$WWWROOT/_framework/$bfile"
    }  >> "$file"
  fi
done <<< "$(find "$WWWROOT/_framework" -type f)"