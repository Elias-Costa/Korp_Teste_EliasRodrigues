FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["services/billing-service/BillingService.csproj", "services/billing-service/"]
RUN dotnet restore "services/billing-service/BillingService.csproj"

COPY . .
WORKDIR "/src/services/billing-service"
RUN dotnet publish "BillingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BillingService.dll"]
