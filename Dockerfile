# syntax=docker/dockerfile:1
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.402 AS build
WORKDIR /source
COPY global.json Directory.Packages.props ./
COPY src/ ./src/
RUN dotnet publish src/HelpBoard.Api/HelpBoard.Api.csproj \
    --configuration Release --output /app/publish \
    --self-contained false -p:UseAppHost=false
RUN dotnet publish src/HelpBoard.Web/HelpBoard.Web.csproj \
    --configuration Release --output /app/client

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
USER root
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish/ ./
COPY --from=build /app/client/wwwroot/ ./wwwroot/
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER $APP_UID
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl --fail --silent --show-error http://localhost:8080/healthz || exit 1
ENTRYPOINT ["dotnet", "HelpBoard.Api.dll"]
