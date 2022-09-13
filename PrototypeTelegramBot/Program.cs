using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace PrototypeTelegramBot
{
    internal class Program
    {
        //api токен бота
        private static string token;
        //статический экземпляр для работы с API
        internal static TelegramBotClient _client;
        //класс обработки команд
        private static CommandService commandService;

        //префикс команд
        internal static string commandPrefix;

        //данные для подключения к бд
        internal static string host = "";
        internal static string port = "";
        internal static string database = "";
        internal static string uid = "";
        internal static string password = "";

        internal static Chat lastChat;
        static void Main(string[] args)
        {
            //проверка существования конфига
            if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "\\config.json"))
            {
                string serialized = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\config.json");
                dynamic json = JObject.Parse(serialized);
                token = json.token;
                commandPrefix = json.commandPrefix;
                host = json.host;
                port = json.port;
                database = json.database;
                uid = json.uid;
                password = json.password;
            }
            //инициализация и пр.
            _client = new TelegramBotClient(token);
            _client.StartReceiving();
            DataBase.Init();
            DataBase.StartTimer();
            commandService = new CommandService();
            //событие - обработка команд
            _client.OnUpdate += _client_OnUpdate;
            Console.ReadLine();
            _client.StopReceiving();
        }
        //вызывается при отправке сообщения в канал
        private async static void _client_OnUpdate(object sender, UpdateEventArgs e)
        {
            if (e.Update.Type == UpdateType.ChannelPost)
            {
                Message msg = e.Update.ChannelPost;
                if (msg is null) return;
                lastChat = msg.Chat;
                //если отправ. текст начинается с префикса
                if (msg.Text.StartsWith(commandPrefix))
                {
                    string text = msg.Text;
                    try
                    {
                        //удаление префикса и последующих параметров
                        text = text.Remove(text.IndexOf(" ")).Remove(0, 1);
                    }
                    catch //вызывается если аргументов нету, а вызвана просто команда
                    {
                        text = text.Remove(0, 1);
                    }
                    try
                    {
                        //выполнение команды и отправка параметров в commandService
                        await commandService.Execute(Miscellaneous
                                .returnEnumCommandType
                                (text), msg.Chat, msg.Text);
                    }
                    //блок обработки ошибок (ошибки могут вызваться из Miscellaneous/CommandService)
                    catch (TypeLoadException)
                    {
                        await _client.SendTextMessageAsync(msg.Chat, "Команды не существует.");
                    }
                    catch (EncoderFallbackException)
                    {
                        //входные параметры оказались пусты/невалидные параметры
                        await _client.SendTextMessageAsync(msg.Chat, "Убедитесь в правильном вводе команды.");
                    }
                    catch (FormatException)
                    {
                        await _client.SendTextMessageAsync(msg.Chat, "Убедитесь в валидности аргументов.");
                    }
                    catch (FieldAccessException)
                    {
                        await _client.SendTextMessageAsync(msg.Chat, "Записи с таким ID не существует.");
                    }
                }
            }
        }
    }
}