FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY PermissionsService/PermissionsService.csproj PermissionsService/
RUN dotnet restore PermissionsService/PermissionsService.csproj
COPY . .
WORKDIR /src/PermissionsService
RUN dotnet build PermissionsService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish PermissionsService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PermissionsService.dll"]
