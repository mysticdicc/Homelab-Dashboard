#dotnet setup
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["web/web/web.csproj", "web/web/"]
COPY ["danklibrary/danklibrary.csproj", "danklibrary/"]
COPY ["web/web.Client/web.Client.csproj", "web/web.Client/"]
RUN dotnet restore "./web/web/web.csproj"
COPY . .
WORKDIR "/src/web/web"
RUN dotnet build "./web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

USER $APP_UID
FROM base AS final
WORKDIR /app

USER $APP_UID
COPY --from=publish /app/publish .

#custom config
RUN mkdir ./data

USER root
RUN apt-get update && apt-get install -y iputils-ping
RUN chmod 0777 ./appsettings.json

#finish publish
USER $APP_UID
ENTRYPOINT ["dotnet", "web.dll"]
