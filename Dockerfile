# -----------------------
# Build stage
# -----------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore ClientEcommerce.API.csproj
RUN dotnet publish ClientEcommerce.API.csproj -c Release -o /app/publish

# -----------------------
# Runtime stage
# -----------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ClientEcommerce.API.dll"]