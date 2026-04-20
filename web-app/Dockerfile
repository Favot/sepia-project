FROM node:22 AS build
WORKDIR /app
RUN corepack enable && corepack prepare pnpm@latest --activate
COPY web-app/package.json web-app/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY web-app/ .
RUN pnpm run build

FROM node:22-slim AS prod-deps
WORKDIR /app
RUN corepack enable && corepack prepare pnpm@latest --activate
COPY web-app/package.json web-app/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile --prod

FROM node:22-slim
WORKDIR /app
COPY --from=build /app/dist ./dist
COPY --from=prod-deps /app/node_modules ./node_modules
COPY --from=build /app/package.json ./
ENV PORT=3000
ENV HOST=0.0.0.0
EXPOSE 3000
CMD ["node", "dist/server/server.js"]