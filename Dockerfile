FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5051

ENV ASPNETCORE_URLS=http://+:5051

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Kaesseli.Server/Kaesseli.Server.csproj", "Kaesseli.Server/"]
RUN dotnet restore "Kaesseli.Server/Kaesseli.Server.csproj"
COPY . .
WORKDIR "/src/Kaesseli.Server"
RUN dotnet build "Kaesseli.Server.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Kaesseli.Server.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kaesseli.Server.dll"]
