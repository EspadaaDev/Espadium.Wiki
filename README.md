# Espadium.Wiki

Confluence-подобная вики-платформа: пространства (Spaces), команды (Teams), страницы с редактором, вложения через S3 API (MinIO), поиск, права и экспорт в PDF. Бэкенд — ASP.NET Core 9 (Minimal APIs), Postgres, Redis, Hangfire.

## 🚀 Стек (MVP)
- **Backend**: .NET 9, Minimal APIs, ASP.NET Identity + JWT, EF Core + Npgsql, Serilog, FluentValidation
- **Infra**: PostgreSQL, **Redis**, **MinIO (S3 API)**, Hangfire (фоновые задачи)
- **API tooling**: Swagger (Swashbuckle), Microsoft.AspNetCore.OpenApi
- **Mail**: MailKit (SMTP)
- **Тесты**: xUnit, FluentAssertions
- **Frontend**: Next.js, TailwindCSS

## ⚙️ Запуск в Docker (MVP)

1) Заполните `.env` (пример):

```
POSTGRES_PASSWORD=postgres
ASPNETCORE_URLS=http://0.0.0.0:8080
CONNECTIONSTRINGS__DEFAULT=Host=postgres;Port=5432;Database=wiki;Username=postgres;Password=postgres
REDIS__CONFIGURATION=redis:6379
S3__ENDPOINT=http://minio:9000
S3__BUCKET=wiki
S3__ACCESSKEY=minioadmin
S3__SECRETKEY=minioadmin
S3__USESSL=false
NEXT_PUBLIC_API_URL=/api
```

2) Сборка и запуск:

```
docker compose build
docker compose up
```

Nginx отдаёт фронт на `http://localhost/`, API доступно под префиксом `/api`.

## 🔐 Аутентификация (MVP)

- При успешном входе (`POST /api/auth/login`) фронт получает `accessToken` и сохраняет его в памяти (in-memory store). Refresh-токен устанавливается в httpOnly-cookie.
- На каждой загрузке клиент делает ленивый `POST /api/auth/refresh`. Если ОК — токен обновляется; если 401 — состояние очищается.
- Кнопка «Выйти» вызывает `POST /api/auth/logout`, очищает состояние и редиректит на `/login`.

## 🪣 MinIO (S3)

- Консоль MinIO: `http://localhost:9001`. Логин/пароль берутся из `.env`: `S3__ACCESSKEY` / `S3__SECRETKEY`.
- Бакет `wiki` должен существовать. Если не создан автоматически, создайте его в консоли MinIO и включите CORS (dev): методы GET/PUT/DELETE/HEAD, origin `*`, headers `*`.

