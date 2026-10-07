FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution-level files required for restore (central package management)
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["global.json", "./"]

# Copy project files for layer-cached restore
COPY ["src/ThaiX.Domain/ThaiX.Domain.csproj", "src/ThaiX.Domain/"]
COPY ["src/ThaiX.Application/ThaiX.Application.csproj", "src/ThaiX.Application/"]
COPY ["src/ThaiX.Infrastructure/ThaiX.Infrastructure.csproj", "src/ThaiX.Infrastructure/"]
COPY ["src/ThaiX.Client/ThaiX.Client.csproj", "src/ThaiX.Client/"]
COPY ["src/ThaiX.Presentation/ThaiX.Presentation.csproj", "src/ThaiX.Presentation/"]

RUN dotnet restore "src/ThaiX.Presentation/ThaiX.Presentation.csproj"

# Copy source code (Client & tests excluded via .dockerignore)
COPY ["src/ThaiX.Domain/", "src/ThaiX.Domain/"]
COPY ["src/ThaiX.Application/", "src/ThaiX.Application/"]
COPY ["src/ThaiX.Infrastructure/", "src/ThaiX.Infrastructure/"]
COPY ["src/ThaiX.Client/", "src/ThaiX.Client/"]
COPY ["src/ThaiX.Presentation/", "src/ThaiX.Presentation/"]

# Client-side libs (bootstrap, bootstrap-icons) via LibMan - gitignored, required before build/publish
WORKDIR /src/src/ThaiX.Client
RUN dotnet tool restore && dotnet tool run libman restore

WORKDIR "/src/src/ThaiX.Presentation"
RUN dotnet build "ThaiX.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ThaiX.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ThaiX.Presentation.dll"]
