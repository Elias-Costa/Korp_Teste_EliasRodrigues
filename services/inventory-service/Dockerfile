FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["services/inventory-service/InventoryService.csproj", "services/inventory-service/"]
RUN dotnet restore "services/inventory-service/InventoryService.csproj"

COPY . .
WORKDIR "/src/services/inventory-service"
RUN dotnet publish "InventoryService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InventoryService.dll"]
