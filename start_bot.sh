cd TelegramAudioBot 
docker build -t tel-audio-bot/bot:1 . && docker run --name tel-audio-bot -itd --restart=always tel-audio-bot/bot:1

