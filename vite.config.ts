import { defineConfig } from 'vite';
import { dreamlandPlugin } from 'vite-plugin-dreamland';

export default defineConfig({
	plugins: [dreamlandPlugin()],
	base: './',
	server: {
		headers: {
			"Cross-Origin-Embedder-Policy": "require-corp",
			"Cross-Origin-Opener-Policy": "same-origin"
		},
		strictPort: true,
		port: 5000,
	},
	build: {
		target: "es2022",
	}
});
