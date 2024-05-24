cd "$1" || exit

if which http-server 2> /dev/null; then
  http-server -c1
else
  python3 -m http.server
fi
