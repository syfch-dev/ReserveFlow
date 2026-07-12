FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ReserveFlow.Api/ReserveFlow.Api.csproj", "src/ReserveFlow.Api/"]
COPY ["src/ReserveFlow.Infrastructure/ReserveFlow.Infrastructure.csproj", "src/ReserveFlow.Infrastructure/"]
COPY ["src/ReserveFlow.Application/ReserveFlow.Application.csproj", "src/ReserveFlow.Application/"]
COPY ["src/ReserveFlow.Domain/ReserveFlow.Domain.csproj", "src/ReserveFlow.Domain/"]
RUN dotnet restore "src/ReserveFlow.Api/ReserveFlow.Api.csproj"
COPY . .
WORKDIR "/src/src/ReserveFlow.Api"
RUN dotnet build "ReserveFlow.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReserveFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReserveFlow.Api.dll"]
