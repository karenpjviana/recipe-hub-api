# Dockerfile otimizado para RecipeHub API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["RecipeHub.API/RecipeHub.API.csproj", "RecipeHub.API/"]
COPY ["RecipeHub.Application/RecipeHub.Application.csproj", "RecipeHub.Application/"]
COPY ["RecipeHub.Domain/RecipeHub.Domain.csproj", "RecipeHub.Domain/"]
COPY ["RecipeHub.Infrastructure/RecipeHub.Infrastructure.csproj", "RecipeHub.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "RecipeHub.API/RecipeHub.API.csproj"

# Copiar código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/RecipeHub.API"
RUN dotnet build "RecipeHub.API.csproj" -c Release -o /app/build

# Publicar aplicação
FROM build AS publish
RUN dotnet publish "RecipeHub.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagem final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar porta para Render
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["dotnet", "RecipeHub.API.dll"]
