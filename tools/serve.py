#!/usr/bin/env python3
from http.server import HTTPServer, SimpleHTTPRequestHandler, test
import sys

class CORSRequestHandler (SimpleHTTPRequestHandler):
    def translate_path(self, path):
        if path.startswith("/_framework"):
            return sys.argv[1] + path[len("/_framework"):]
        else:
            return "wwwroot" + path

    def end_headers(self):
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        SimpleHTTPRequestHandler.end_headers(self)

if __name__ == "__main__":
    test(CORSRequestHandler, HTTPServer, port=int(sys.argv[2]) if len(sys.argv) > 2 else 5000)
