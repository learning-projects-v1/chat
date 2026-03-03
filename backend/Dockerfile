FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ChatApp.Domain/ChatApp.Domain.csproj ChatApp.Domain/
COPY ChatApp.Application/ChatApp.Application.csproj ChatApp.Application/
COPY ChatApp.Infrastructure/ChatApp.Infrastructure.csproj ChatApp.Infrastructure/
COPY ChatApp.Api/ChatApp.API.csproj ChatApp.Api/
RUN dotnet restore ChatApp.Api/ChatApp.API.csproj

# Copy everything and publish
COPY . .
RUN dotnet restore ChatApp.Api/ChatApp.API.csproj
RUN dotnet publish ChatApp.Api/ChatApp.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ChatApp.API.dll"]
