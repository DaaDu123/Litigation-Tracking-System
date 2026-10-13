
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["LTSFrontend.csproj", "."]
RUN dotnet restore "LTSFrontend.csproj"

COPY . .
RUN dotnet publish "LTSFrontend.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

RUN mkdir -p /var/aspnet/DataProtection-Keys \
    && chmod 700 /var/aspnet/DataProtection-Keys

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LTSFrontend.dll"]
