FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Notification/Notification.API/Notification.API.csproj", "src/Services/Notification/Notification.API/"]
COPY ["src/BuildingBlocks/CloudCart.BuildingBlocks/CloudCart.BuildingBlocks.csproj", "src/BuildingBlocks/CloudCart.BuildingBlocks/"]
RUN dotnet restore "src/Services/Notification/Notification.API/Notification.API.csproj"
COPY . .
RUN dotnet publish "src/Services/Notification/Notification.API/Notification.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
