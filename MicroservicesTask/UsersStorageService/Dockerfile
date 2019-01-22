FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY UsersStorageService/UsersStorageService.csproj UsersStorageService/
RUN dotnet restore UsersStorageService/UsersStorageService.csproj
COPY . .
WORKDIR /src/UsersStorageService
RUN dotnet build UsersStorageService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish UsersStorageService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "UsersStorageService.dll"]
