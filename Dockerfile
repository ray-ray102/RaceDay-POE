# RaceDay MVC web app - multi-stage .NET 8 image
# Build from the repository root:
#   docker build -t raceday-web -f Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first for better layer caching
COPY RaceDay-POE-Web/RaceDay-POE-Web.csproj RaceDay-POE-Web/
RUN dotnet restore RaceDay-POE-Web/RaceDay-POE-Web.csproj

COPY RaceDay-POE-Web/ RaceDay-POE-Web/
RUN dotnet publish RaceDay-POE-Web/RaceDay-POE-Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Non-root user for a safer runtime
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
# Override with -e ApiSettings__BaseUrl=http://host:5141/ when running
ENV ApiSettings__BaseUrl=http://host.docker.internal:5141/

EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay-POE-Web.dll"]
