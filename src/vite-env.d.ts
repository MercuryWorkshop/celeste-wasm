/// <reference types="vite/client" />

interface ImportMetaEnv {
	readonly VITE_DECRYPT_ENABLED: string
	readonly VITE_DECRYPT_PATH: string
	readonly VITE_DECRYPT_KEY: string
	readonly VITE_DECRYPT_SIZE: string
	readonly VITE_DECRYPT_COUNT: string
}

interface ImportMeta {
	readonly env: ImportMetaEnv
}
