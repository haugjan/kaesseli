FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY Directory.Packages.props .
COPY Kaesseli.Contracts/Kaesseli.Contracts.csproj Kaesseli.Contracts/
COPY Kaesseli/Kaesseli.csproj Kaesseli/
COPY Kaesseli.Client.Blazor/Kaesseli.Client.Blazor.csproj Kaesseli.Client.Blazor/
RUN dotnet restore Kaesseli/Kaesseli.csproj
RUN dotnet restore Kaesseli.Client.Blazor/Kaesseli.Client.Blazor.csproj

COPY Kaesseli.Contracts/ Kaesseli.Contracts/
COPY Kaesseli/ Kaesseli/
COPY Kaesseli.Client.Blazor/ Kaesseli.Client.Blazor/

RUN dotnet publish Kaesseli/Kaesseli.csproj -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish Kaesseli.Client.Blazor/Kaesseli.Client.Blazor.csproj -c Release -o /app/blazor

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/blazor/wwwroot wwwroot/
ENTRYPOINT ["dotnet", "Kaesseli.dll"]
