# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the nested project folder
COPY CarRentalAPI/ ./CarRentalAPI/

# Restore using the correct nested path
RUN dotnet restore CarRentalAPI/CarRentalAPI.csproj

# Publish
RUN dotnet publish CarRentalAPI/CarRentalAPI.csproj -c Release -o /app/publish

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5020

ENTRYPOINT ["dotnet", "CarRentalAPI.dll"]