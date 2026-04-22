# Use the .NET 10 SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["RestApi/RestApi.csproj", "RestApi/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Sheard/Sheard.csproj", "Sheard/"]
RUN dotnet restore "RestApi/RestApi.csproj"

# Copy the remaining files and build the application
COPY . .
WORKDIR "/src/RestApi"
RUN dotnet build "RestApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "RestApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the .NET 10 ASP.NET Core runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestApi.dll"]
