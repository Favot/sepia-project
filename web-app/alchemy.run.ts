/// <reference types="@types/node" />

import alchemy from "alchemy";
import { TanStackStart } from "alchemy/cloudflare";

const app = await alchemy("sepia-project");

export const worker = await TanStackStart("website", {
  build: { command: "node ./node_modules/vite-plus/bin/vp build" },
  vars: {
    VITE_API_URL: process.env.VITE_API_URL!,
  },
});

console.log({ url: worker.url });

await app.finalize();