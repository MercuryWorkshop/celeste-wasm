import epoxyInit, {
	EpoxyClient,
	EpoxyClientOptions,
	EpoxyHandlers,
	EpoxyIoStream,
	EpoxyWebSocket,
	info as epoxyInfo,
} from "@mercuryworkshop/epoxy-tls/epoxy";
import EPOXY_PATH from "../../node_modules/@mercuryworkshop/epoxy-tls/full/epoxy.wasm?url";
import { store } from "./store";

export let epoxyVersion =
	epoxyInfo.version + epoxyInfo.commit + epoxyInfo.release;

let cache: Cache = await window.caches.open("epoxy");
let initted: boolean = false;

let currentClient: EpoxyClient;
let currentWispUrl: string;

async function evict() {
	await cache.delete(EPOXY_PATH);
}

async function instantiate() {
	try {
		if (!(await cache.match(EPOXY_PATH))) await cache.add(EPOXY_PATH);
		await epoxyInit({ module_or_path: await cache.match(EPOXY_PATH) });
		initted = true;
	} catch (err) {
		initted = false;
		try {
			await evict();
		} catch {}
		throw err;
	}
}

async function tryInit() {
	if (!initted) {
		if (epoxyVersion === store.epoxyVersion) {
			await instantiate();
		} else {
			await evict();
			await instantiate();
			console.log(
				`evicted epoxy "${store.epoxyVersion}" from cache because epoxy "${epoxyVersion}" is available`
			);
			store.epoxyVersion = epoxyVersion;
		}
	}

	if (currentWispUrl !== store.wispServer) {
		await createEpoxy();
	}
}

export function getWispUrl() {
	return currentWispUrl;
}

export async function createEpoxy() {
	let options = new EpoxyClientOptions();
	options.user_agent =
		navigator.userAgent +
		" Celeste/1.4.0.0.WASM CelesteWasm/" +
		location.hostname;
	options.udp_extension_required = false;

	currentWispUrl = store.wispServer;
	currentClient = new EpoxyClient(currentWispUrl, options);
}

export async function epoxyFetch(
	url: string,
	options?: any
): Promise<Response> {
	await tryInit();

	try {
		return await currentClient.fetch(url, options);
	} catch (err2) {
		let err = err2 as Error;
		console.log(err);

		throw err;
	}
}

const WebSocketFields = {
	prototype: {
		send: WebSocket.prototype.send,
	},
	CLOSED: WebSocket.CLOSED,
	CLOSING: WebSocket.CLOSING,
	CONNECTING: WebSocket.CONNECTING,
	OPEN: WebSocket.OPEN,
};

// from bare-mux
export class EpxTcpWs extends EventTarget {
	url: string;
	readyState: number = WebSocketFields.CONNECTING;

	ws?: WritableStreamDefaultWriter;

	binaryType: "blob" | "arraybuffer" = "blob";

	bufferedAmount: number = 0;

	onopen?: (evt: Event) => void;
	onclose?: (evt: Event) => void;
	onmessage?: (evt: Event) => void;
	onerror?: (evt: Event) => void;

	realOnClose: (code: number, reason: string) => void;

	constructor(remote: string | URL, type: string) {
		super();

		console.log("tcpws", remote.toString(), type);

		this.url = remote.toString();

		const onopen = () => {
			this.readyState = WebSocketFields.OPEN;

			const event = new Event("open");
			this.dispatchEvent(event);

			if (this.onopen) this.onopen(event);
		};

		const onmessage = async (payload: Uint8Array) => {
			let data;
			if (this.binaryType === "blob") data = new Blob([payload]);
			else if (this.binaryType === "arraybuffer") data = payload.buffer;

			const event = new MessageEvent("message", { data });
			this.dispatchEvent(event);
			if (this.onmessage) this.onmessage(event);
		};

		const onclose = (code: number, reason: string) => {
			this.readyState = WebSocketFields.CLOSED;
			const event = new CloseEvent("close", { code, reason });
			this.dispatchEvent(event);
			if (this.onclose) this.onclose(event);
		};
		this.realOnClose = onclose;

		const onerror = () => {
			this.readyState = WebSocketFields.CLOSED;
			const event = new Event("error");
			this.dispatchEvent(event);
			if (this.onerror) this.onerror(event);
		};

		if (!["tcp", "udp", "tls"].includes(type)) throw "invalid";

		(async () => {
			await tryInit();

			let ws: EpoxyIoStream = null!;
			try {
				if (type === "tcp") {
					ws = await currentClient.connect_tcp(remote);
				} else if (type === "udp") {
					ws = await currentClient.connect_udp(remote);
				} else if (type === "tls") {
					ws = await currentClient.connect_tls(remote);
				}
			} catch (err) {
				console.error(err);
				onerror();
				return;
			}

			this.ws = ws.write.getWriter();

			const reader = ws.read.getReader();

			this.readyState = WebSocketFields.OPEN;
			onopen();
			let errored = false;
			while (true) {
				try {
					const { value, done } = await reader.read();
					if (done || !value) break;

					onmessage(value);
				} catch (err) {
					onerror();
					console.error(err);
					errored = true;
					break;
				}
			}
			this.readyState = WebSocketFields.CLOSED;
			onclose(errored ? 1011 : 1000, errored ? "epoxy.ts errored" : "normal");
		})();
	}

	send(...args: any[]) {
		if (this.readyState === WebSocketFields.CONNECTING || !this.ws) {
			throw new DOMException(
				"Failed to execute 'send' on 'WebSocket': Still in CONNECTING state."
			);
		}

		let data = args[0];
		if (data.buffer)
			data = data.buffer.slice(
				data.byteOffset,
				data.byteOffset + data.byteLength
			);

		this.bufferedAmount++;
		this.ws.write(data).then(() => {
			this.bufferedAmount--;
		});
	}

	close(_: number, __: string) {
		if (this.readyState !== WebSocketFields.OPEN) return;

		console.log("closing");
		this.readyState = WebSocketFields.CLOSING;

		this.ws?.close().then((x) => console.log("really closed", x));

		console.log("closed");
		this.readyState = WebSocketFields.CLOSED;
		this.realOnClose(1000, "normal");
	}
}

export class EpxWs extends EventTarget {
	url: string;
	readyState: number = WebSocketFields.CONNECTING;
	protocols: string | string[] | undefined;

	ws?: EpoxyWebSocket;

	binaryType: "blob" | "arraybuffer" = "blob";

	bufferedAmount: number = 0;

	onopen?: (evt: Event) => void;
	onclose?: (evt: Event) => void;
	onmessage?: (evt: Event) => void;
	onerror?: (evt: Event) => void;

	constructor(
		remote: string | URL,
		protocols: string | string[] | undefined = []
	) {
		super();

		this.url = remote.toString();
		this.protocols = protocols;

		const onopen = () => {
			this.readyState = WebSocketFields.OPEN;

			const event = new Event("open");
			this.dispatchEvent(event);
			if (this.onopen) this.onopen(event);
		};

		const onmessage = async (payload: Uint8Array) => {
			let data;
			if (this.binaryType === "blob") data = new Blob([payload]);
			else if (this.binaryType === "arraybuffer") data = payload.buffer;

			const event = new MessageEvent("message", { data });
			this.dispatchEvent(event);
			if (this.onmessage) this.onmessage(event);
		};

		const onclose = (code: number, reason: string) => {
			this.readyState = WebSocketFields.CLOSED;
			const event = new CloseEvent("close", { code, reason });
			this.dispatchEvent(event);
			if (this.onclose) this.onclose(event);
		};

		const onerror = () => {
			this.readyState = WebSocketFields.CLOSED;
			const event = new Event("error");
			this.dispatchEvent(event);
			if (this.onerror) this.onerror(event);
		};

		(async () => {
			await tryInit();
			const handlers = new EpoxyHandlers(onopen, onclose, onerror, onmessage);

			let protos;
			if (typeof protocols === "string") {
				protos = [protocols];
			} else {
				protos = protocols;
			}

			try {
				this.ws = await currentClient.connect_websocket(
					handlers,
					remote,
					protos,
					{}
				);
			} catch (err) {
				console.error(err);
				onerror();
				return;
			}
		})();
	}

	send(...args: any[]) {
		if (this.readyState === WebSocketFields.CONNECTING || !this.ws) {
			throw new DOMException(
				"Failed to execute 'send' on 'WebSocket': Still in CONNECTING state."
			);
		}

		let data = args[0];
		if (data.buffer)
			data = data.buffer.slice(
				data.byteOffset,
				data.byteOffset + data.byteLength
			);

		this.bufferedAmount++;
		this.ws.send(data).then(() => {
			this.bufferedAmount--;
		});
	}

	close(code: number, reason: string) {
		this.ws?.close(code, reason);
	}
}

(self as any).epoxyFetch = epoxyFetch;
