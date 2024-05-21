import sys

file = sys.argv[1]
key = sys.argv[2]

with open(file, 'rb') as f, open(key, 'rb') as k:
    file = bytearray(f.read())
    key = bytearray(k.read())

    i = 0
    while i < len(file):
        file[i] ^= key[i % len(key)]
        i += 4096

    sys.stdout.buffer.write(file)
