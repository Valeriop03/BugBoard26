FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/BugBoard26.Api/BugBoard26.Api.csproj backend/BugBoard26.Api/
RUN dotnet restore backend/BugBoard26.Api/BugBoard26.Api.csproj

COPY backend/BugBoard26.Api/ backend/BugBoard26.Api/
RUN dotnet publish backend/BugBoard26.Api/BugBoard26.Api.csproj \
    --configuration Release --output /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "BugBoard26.Api.dll"]
