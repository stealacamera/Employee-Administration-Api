#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EmployeeAdministration.API/EmployeeAdministration.API.csproj", "EmployeeAdministration.API/"]
COPY ["EmployeeAdministration.Application/EmployeeAdministration.Application.csproj", "EmployeeAdministration.Application/"]
COPY ["EmployeeAdministration.Infrastructure/EmployeeAdministration.Infrastructure.csproj", "EmployeeAdministration.Infrastructure/"]
COPY ["EmployeeAdministration.Domain/EmployeeAdministration.Domain.csproj", "EmployeeAdministration.Domain/"]
RUN dotnet restore "./EmployeeAdministration.API/EmployeeAdministration.API.csproj"
COPY . .
WORKDIR "/src/EmployeeAdministration.API"
RUN dotnet build "./EmployeeAdministration.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EmployeeAdministration.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeAdministration.API.dll"]