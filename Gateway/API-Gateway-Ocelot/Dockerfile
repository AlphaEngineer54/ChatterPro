# Image runtime ASP.NET Core 8.0 - phase base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["API-Gateway-Ocelot.csproj", "./"]
RUN dotnet restore "API-Gateway-Ocelot.csproj"

COPY . .

RUN dotnet build "API-Gateway-Ocelot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API-Gateway-Ocelot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API-Gateway-Ocelot.dll"]
