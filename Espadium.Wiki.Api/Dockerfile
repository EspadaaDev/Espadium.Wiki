FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine

WORKDIR /app

# Expect published output to be copied in
COPY ./bin/Release/net9.0/publish/ ./

ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

EXPOSE 8080

ENTRYPOINT ["dotnet", "Espadium.Wiki.Api.dll"]

