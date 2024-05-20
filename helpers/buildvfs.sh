emsdk=$(dirname "$(which emcc)")
file_packager="$emsdk/tools/file_packager"
wwwroot=$1

"$file_packager" data.data --preload Content/@/Content --js-output="data.js.tmp" --lz4 --no-node --use-preload-cache
echo packed
mv data.data "$wwwroot/_framework/data.data"
sed -i "2d" data.js.tmp
content=$(<data.js.tmp)
# if this line is commented out it is a mistake (i forgot to recomment it in)
content=${content/\.data\'\);/.data\'); doneCallback();}

echo "function loadData(Module, doneCallback) {" > "$wwwroot/data.js"
echo "$content" >> "$wwwroot/data.js"
echo "}" >> "$wwwroot/data.js"
rm data.js.tmp
