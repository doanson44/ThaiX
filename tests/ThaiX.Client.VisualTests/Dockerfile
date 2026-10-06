# ThaiX.Client host for Playwright visual tests (Blazor WASM DevServer + ForceDemo).
# Built by AuthFixture via Testcontainers ImageFromDockerfileBuilder.
# Build context: repository root (see ClientAppHost).
#
# Uses DevServer (not nginx) so fingerprinted blazor.webassembly.js assets resolve.

FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /workspace

# Central package versions only — do NOT copy repo global.json / Directory.Build.props
# (those pin SDK 6 / TFM net6 and break this net10 client).
COPY Directory.Packages.props ./
COPY src/ThaiX.Client/ThaiX.Client.csproj src/ThaiX.Client/

WORKDIR /workspace/src/ThaiX.Client
RUN dotnet restore ThaiX.Client.csproj

WORKDIR /workspace
COPY src/ThaiX.Client/ src/ThaiX.Client/

WORKDIR /workspace/src/ThaiX.Client
# Client-side libs (bootstrap, bootstrap-icons) via LibMan — required before build/run.
RUN dotnet tool restore \
 && dotnet tool run libman restore

RUN dotnet build ThaiX.Client.csproj -c Release --no-restore

ENV ASPNETCORE_ENVIRONMENT=VisualTests \
    ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1

EXPOSE 8080

ENTRYPOINT ["dotnet", "run", "--project", "ThaiX.Client.csproj", "-c", "Release", "--no-build", "--no-launch-profile", "--urls", "http://0.0.0.0:8080"]
