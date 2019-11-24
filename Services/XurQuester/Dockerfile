FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Services/XurQuester/XurQuester.csproj", "Services/XurQuester/"]
RUN dotnet restore "Services/XurQuester/XurQuester.csproj"
COPY . .
WORKDIR "/src/Services/XurQuester"
RUN dotnet build "XurQuester.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "XurQuester.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "XurQuester.dll"]