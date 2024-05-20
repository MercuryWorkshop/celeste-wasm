# hex | -> binary
fromhex() {
	xxd -p -r -c999999
}

# binary | -> hex
tohex() {
	xxd -p
}

# (number) -> hex
toint() {
	printf "%08x" "$1"
}
