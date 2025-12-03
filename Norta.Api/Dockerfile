FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Norta.Api/Norta.Api.csproj", "Norta.Api/"]
RUN dotnet restore "Norta.Api/Norta.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Norta.Api"
RUN dotnet build "Norta.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Norta.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Norta.Api.dll"]
