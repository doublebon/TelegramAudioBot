cd TelegramAudioBot\TelegramAudioBot 
docker build -t telbot/bot:1 . && docker run --name telbot -itd --restart=always telbot/bot:1

