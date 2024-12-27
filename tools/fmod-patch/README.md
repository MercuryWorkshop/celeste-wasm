# fmod patches for multithreading
proxies all fmod (studio) calls to main thread (which is unused by dotnet)

1. copy all the fmod headers into `headers/`
2. copy `fmodstudio.a` into this folder
3. run `compile.fish`

to use, `sed -i 's/FMOD_Studio_/WRAP_FMOD_Studio_/' fmod_studio.cs`

