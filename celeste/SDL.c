#include <stddef.h>

void SDL_AndroidBackButton(void) {}
void * SDL_AndroidGetActivity(void) { return NULL; }
const char * SDL_AndroidGetExternalStoragePath(void) { return NULL; }
int SDL_AndroidGetExternalStorageState(void) { return 0; }
const char * SDL_AndroidGetInternalStoragePath(void) { return NULL; }
void * SDL_AndroidGetJNIEnv(void) { return NULL; }
int SDL_AndroidRequestPermission(const char *permission) { return 0; }
int SDL_AndroidShowToast(const char* message, int duration, int gravity, int xoffset, int yoffset) { return 0; }
int SDL_GDKRunApp(void *mainFunction, void *reserved) { return -1; }
int SDL_GetAndroidSDKVersion(void) { return 10; }
int SDL_iPhoneSetAnimationCallback(void * window, int interval, void *callback, void *callbackParam) { return -1; }
void SDL_iPhoneSetEventPump(int enabled) {}
int SDL_IsAndroidTV(void) { return 0; }
int SDL_IsChromebook(void) { return 1; }
int SDL_IsDeXMode(void) { return 0; }
void* SDL_RenderGetD3D11Device(void * renderer) { return NULL; }
void* SDL_RenderGetD3D9Device(void * renderer) { return NULL; }
void SDL_SetWindowsMessageHook(void *callback, void *userdata) {}
int SDL_UIKitRunApp(int argc, char *argv[], void *mainFunction) { return -1; }
int SDL_WinRTGetDeviceFamily() { return 0; }
int SDL_WinRTRunApp(void *mainFunction, void * reserved) { return -1; }
