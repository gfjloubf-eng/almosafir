# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files
COPY AlMosafer.slnx ./
COPY src/AlMosafer.Domain/*.csproj ./src/AlMosafer.Domain/
COPY src/AlMosafer.Application/*.csproj ./src/AlMosafer.Application/
COPY src/AlMosafer.Infrastructure/*.csproj ./src/AlMosafer.Infrastructure/
COPY src/AlMosafer.Web/*.csproj ./src/AlMosafer.Web/
COPY tests/AlMosafer.Tests/*.csproj ./tests/AlMosafer.Tests/

# Restore dependencies
RUN dotnet restore AlMosafer.slnx

# Copy all source files
COPY . ./

# Build and Publish
RUN dotnet publish src/AlMosafer.Web/AlMosafer.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Environment Defaults for Container Runtime
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AlMosafer.Web.dll"]
