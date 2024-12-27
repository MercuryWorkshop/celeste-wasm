fish wrap.fish >wrapped.c
emcc -r wrapped.c -o wrapped.o
mkdir fmodstudio
cd fmodstudio
ar x ../fmodstudio.a
mv ../wrapped.o .
emar rc ../fmodstudio-wrapped.a *.o
cd ..
rm -r fmodstudio
