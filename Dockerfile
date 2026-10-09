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
ENV IssueImages__Directory=/var/lib/bugboard26/issue-images
EXPOSE 8080

RUN mkdir -p /var/lib/bugboard26/issue-images \
    && chown "${APP_UID}:${APP_UID}" /var/lib/bugboard26/issue-images \
    && chmod 700 /var/lib/bugboard26/issue-images

COPY --from=build /app/publish .
USER $APP_UID
RUN test "$(id -u)" != "0" && test -w /var/lib/bugboard26/issue-images
ENTRYPOINT ["dotnet", "BugBoard26.Api.dll"]
