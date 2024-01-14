FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5281

ENV ASPNETCORE_URLS=http://+:5281

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["EchoServer/EchoServer.csproj", "EchoServer/"]
RUN dotnet restore "EchoServer/EchoServer.csproj"
COPY . .
WORKDIR "/src/EchoServer"
RUN dotnet build "EchoServer.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "EchoServer.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
RUN apt-get update && apt-get install -y curl iputils-ping
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EchoServer.dll"]
