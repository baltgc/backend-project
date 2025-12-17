# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.slnx .
COPY NetChallenge.API/*.csproj ./NetChallenge.API/
COPY NetChallenge.Domain/*.csproj ./NetChallenge.Domain/
COPY NetChallenge.Application/*.csproj ./NetChallenge.Application/
COPY NetChallenge.Infrastructure/*.csproj ./NetChallenge.Infrastructure/
COPY NetChallenge.Application.Tests/*.csproj ./NetChallenge.Application.Tests/

# Restore dependencies
RUN dotnet restore NetChallenge.slnx

# Copy everything else and build
COPY . .
WORKDIR /src
RUN dotnet build NetChallenge.slnx -c Release --no-restore

# Publish the application
FROM build AS publish
WORKDIR /src/NetChallenge.API
RUN dotnet publish -c Release -o /app/publish

# Use the runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app from build stage
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NetChallenge.API.dll"]

