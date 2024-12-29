#include <emscripten/console.h>
#include <emscripten/wasmfs.h>
#include <emscripten/proxying.h>
#include <emscripten/threading.h>
#include <assert.h>

int mount_opfs() {
	emscripten_console_log("mount_opfs: starting");
	backend_t opfs = wasmfs_create_opfs_backend();
	emscripten_console_log("mount_opfs: created opfs backend");
	int ret = wasmfs_create_directory("/libsdl", 0777, opfs);
	emscripten_console_log("mount_opfs: mounted opfs");
	return ret;
}
