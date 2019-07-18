FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["OcelotApiGateway/OcelotApiGateway.csproj", "OcelotApiGateway/"]
RUN dotnet restore "OcelotApiGateway/OcelotApiGateway.csproj"
COPY . .
WORKDIR "/src/OcelotApiGateway"
RUN dotnet build "OcelotApiGateway.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "OcelotApiGateway.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "OcelotApiGateway.dll"]