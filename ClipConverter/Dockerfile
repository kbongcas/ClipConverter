#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
RUN apt-get update
RUN apt-get install -y ffmpeg
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ClipConverter/ClipConverter.csproj", "ClipConverter/"]
RUN dotnet restore "ClipConverter/ClipConverter.csproj"
COPY . .
WORKDIR "/src/ClipConverter"
RUN dotnet build "ClipConverter.csproj" -c Release -o /app/build
RUN mkdir -p /data/converted
RUN mkdir -p /data/clips


FROM build AS publish
RUN dotnet publish "ClipConverter.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClipConverter.dll"]