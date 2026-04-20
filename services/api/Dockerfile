FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY services/common/*.csproj ./services/common/
RUN dotnet restore services/common/Common.csproj
COPY services/api/*.csproj ./services/api/
RUN dotnet restore services/api/Api.csproj
COPY services/ ./services/
RUN dotnet publish services/api/Api.csproj -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 3000
ENTRYPOINT ["dotnet", "Api.dll"]
