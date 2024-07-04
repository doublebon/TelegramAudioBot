FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Install ffmpeg from the official repository with no recommended packages
RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . "TelegramAudioBot"
RUN dotnet restore "TelegramAudioBot/TelegramAudioBot.csproj"
COPY . .
WORKDIR "/src/TelegramAudioBot"
RUN dotnet build "TelegramAudioBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramAudioBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramAudioBot.dll"]
