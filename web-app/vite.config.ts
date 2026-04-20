import { defineConfig } from "vite-plus";
import { tanstackStart } from "@tanstack/react-start/plugin/vite";
import viteReact from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import alchemy from "alchemy/cloudflare/tanstack-start";

const config = defineConfig({
  lint: { options: { typeAware: true, typeCheck: true } },
  resolve: { tsconfigPaths: true },
  build: {
    target: "esnext",
    rollupOptions: {
      external: ["node:async_hooks", "cloudflare:workers"],
    },
  },
  plugins: [
    alchemy(),
    tailwindcss(),
    tanstackStart({
      target: "cloudflare-module",
      customViteReactPlugin: true,
    }),
    viteReact(),
  ],
});

export default config;