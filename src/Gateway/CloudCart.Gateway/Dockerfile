FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Gateway/CloudCart.Gateway/CloudCart.Gateway.csproj", "src/Gateway/CloudCart.Gateway/"]
RUN dotnet restore "src/Gateway/CloudCart.Gateway/CloudCart.Gateway.csproj"
COPY . .
RUN dotnet publish "src/Gateway/CloudCart.Gateway/CloudCart.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CloudCart.Gateway.dll"]