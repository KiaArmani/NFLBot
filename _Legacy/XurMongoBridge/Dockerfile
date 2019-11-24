FROM mcr.microsoft.com/dotnet/core/aspnet:2.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build
WORKDIR /src
COPY ["xurmongobridge.csproj", "./"]

RUN dotnet restore "./xurmongobridge.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "xurmongobridge.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "xurmongobridge.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "xurmongobridge.dll"]