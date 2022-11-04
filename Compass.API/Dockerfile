#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Compass.API/Compass.API.csproj", "Compass.API/"]
COPY ["Services/Compass.Services.csproj", "Services/"]
COPY ["Compass.Data/Compass.Data.csproj", "Compass.Data/"]
RUN dotnet restore "Compass.API/Compass.API.csproj"
COPY . .
WORKDIR "/src/Compass.API"
RUN dotnet build "Compass.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Compass.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Compass.API.dll"]