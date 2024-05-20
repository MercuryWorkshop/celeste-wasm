cd "$1" || exit

if which http-server > /dev/null; then
  http-server -c1
else
  python3 -m http.server
fi
