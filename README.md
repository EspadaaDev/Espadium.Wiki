# Espadium.Wiki

Confluence-подобная вики-платформа: пространства (Spaces), команды (Teams), страницы с редактором, вложения через S3 API (MinIO), поиск, права и экспорт в PDF. Бэкенд — ASP.NET Core 9 (Minimal APIs), Postgres, Redis, Hangfire.

## 🚀 Стек (MVP)
- **Backend**: .NET 9, Minimal APIs, ASP.NET Identity + JWT, EF Core + Npgsql, Serilog, FluentValidation
- **Infra**: PostgreSQL, **Redis**, **MinIO (S3 API)**, Hangfire (фоновые задачи)
- **API tooling**: Swagger (Swashbuckle), Microsoft.AspNetCore.OpenApi
- **Mail**: MailKit (SMTP)
- **Тесты**: xUnit, FluentAssertions
- **Frontend** (позже, в `/web`): Next.js, TailwindCSS, shadcn/ui, TipTap, Excalidraw
