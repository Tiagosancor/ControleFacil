FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:306301580fcaa5b445180e759db59309979002d1000669cb4cf58a567d0014bc AS build
WORKDIR /src

COPY ControleFacil.sln ./
COPY src/ControleFacil.Domain/ControleFacil.Domain.csproj src/ControleFacil.Domain/
COPY src/ControleFacil.Application/ControleFacil.Application.csproj src/ControleFacil.Application/
COPY src/ControleFacil.Infrastructure/ControleFacil.Infrastructure.csproj src/ControleFacil.Infrastructure/
COPY src/ControleFacil.Api/ControleFacil.Api.csproj src/ControleFacil.Api/
COPY tests/ControleFacil.Api.Tests/ControleFacil.Api.Tests.csproj tests/ControleFacil.Api.Tests/
RUN dotnet restore src/ControleFacil.Api/ControleFacil.Api.csproj

COPY . .
RUN dotnet publish src/ControleFacil.Api/ControleFacil.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:b0beb9cc1dee1c1b0749796110d4734292071b814207ad0d4f40611f7db04f7b AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ControleFacil.Api.dll"]
