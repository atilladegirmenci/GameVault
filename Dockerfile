
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file and restore dependencies
COPY ["GameDBWebApi/GameDBWebApi.csproj", "GameDBWebApi/"]

RUN dotnet restore "GameDBWebApi/GameDBWebApi.csproj"

# Copy source code
COPY . .

# Publish application
WORKDIR /src/GameDBWebApi

RUN dotnet publish "GameDBWebApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GameDBWebApi.dll"]
