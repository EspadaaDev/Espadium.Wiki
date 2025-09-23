# === build stage ===
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# копируем sln и csproj'и заранее для кэшируемого restore
COPY ../Espadium.Wiki.sln ./
COPY Espadium.Wiki.Api/Espadium.Wiki.Api.csproj Espadium.Wiki.Api/
COPY Espadium.Wiki.Application/Espadium.Wiki.Application.csproj Espadium.Wiki.Application/
COPY Espadium.Wiki.Domain/Espadium.Wiki.Domain.csproj Espadium.Wiki.Domain/
COPY Espadium.Wiki.Infrastructure/Espadium.Wiki.Infrastructure.csproj Espadium.Wiki.Infrastructure/
RUN dotnet restore Espadium.Wiki.Api/Espadium.Wiki.Api.csproj

# копируем остальной код и публикуем
COPY . .
RUN dotnet publish Espadium.Wiki.Api/Espadium.Wiki.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# === runtime stage ===
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Espadium.Wiki.Api.dll"]
