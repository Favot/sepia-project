FROM node:22 AS build
WORKDIR /app
RUN corepack enable && corepack prepare pnpm@latest --activate
COPY web-app/package.json web-app/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY web-app/ .
RUN pnpm run build

FROM node:22-slim
WORKDIR /app
RUN corepack enable && corepack prepare pnpm@latest --activate
COPY web-app/package.json web-app/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY --from=build /app/dist ./dist
COPY web-app/vite.config.ts web-app/tsconfig.json ./
ENV PORT=3000
EXPOSE 3000
CMD ["npx", "vp", "preview", "--port", "3000", "--host", "0.0.0.0"]
