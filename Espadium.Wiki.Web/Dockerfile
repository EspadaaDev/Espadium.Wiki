# === deps (для кэша npm ci) ===
FROM node:20-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci

# === build ===
FROM node:20-alpine AS build
WORKDIR /app
ENV NEXT_TELEMETRY_DISABLED=1
COPY --from=deps /app/node_modules ./node_modules
COPY . .
# переменная base URL передаётся в контейнер через compose (NEXT_PUBLIC_API_URL)
RUN npm run build

# === runtime ===
FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
EXPOSE 3000
# копируем только нужное для прод запуска
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/package.json ./package.json
COPY --from=deps  /app/node_modules ./node_modules
CMD ["npm","run","start"]
