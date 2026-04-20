FROM node:22
WORKDIR /app
RUN corepack enable && corepack prepare pnpm@latest --activate
COPY web-app/package.json web-app/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY web-app/ .
RUN pnpm run build

ENV PORT=3000
ENV HOST=0.0.0.0
EXPOSE 3000
CMD ["node", "dist/server/server.js"]