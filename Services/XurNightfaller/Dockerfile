FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Services/XurNightfaller/XurNightfaller.csproj", "Services/XurNightfaller/"]
RUN dotnet restore "Services/XurNightfaller/XurNightfaller.csproj"
COPY . .
WORKDIR "/src/Services/XurNightfaller"
RUN dotnet build "XurNightfaller.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "XurNightfaller.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "XurNightfaller.dll"]