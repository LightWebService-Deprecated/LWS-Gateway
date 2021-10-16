FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["LWS-Gateway/LWS-Gateway.csproj", "LWS-Gateway/"]
RUN dotnet restore "LWS-Gateway/LWS-Gateway.csproj"
COPY . .
WORKDIR "/src/LWS-Gateway"
RUN dotnet build "LWS-Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LWS-Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LWS-Gateway.dll"]
