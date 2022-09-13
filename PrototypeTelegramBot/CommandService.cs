using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace PrototypeTelegramBot
{
    internal enum commandType
    {
        add,
        del,
        sel,
        help,
        undefined,
    }
    internal class CommandService
    {
        //обработчик команд (вызывается из Program.cs при вызове команды)
        public async Task Execute(commandType CT, Chat chatToReply, string arg)
        {
            switch (CT)
            {
                case commandType.add:
                    await Add(Miscellaneous.separateRows(arg), chatToReply);
                    break;
                case commandType.del:
                    await Delete(Miscellaneous.getId(arg), chatToReply);
                    break;
                case commandType.sel:
                    await Sel(Miscellaneous.getId(arg), chatToReply);
                    break;
                case commandType.help:
                    await Help(chatToReply);
                    break;
            }
        }
        //добавление позиции
        private async Task Add(List<row> rows, Chat chat)
        {
            if (rows == null) throw new FormatException();
            string id_rows = "";
            foreach (row r in rows)
            {
                if (r.name == null || r.phone == null || r.date == null)
                {
                    //входные параметры оказались пусты/невалидные параметры
                    throw new EncoderFallbackException();
                }
                int id = await DataBase.addRow(r.date, r.name, r.phone);
                id_rows += id + ";";
            }
            await Program._client.SendTextMessageAsync(chat, $"Успешно добавлено. \r\n" +
                $"Позиций: {rows.Count}\r\n" +
                $"ID: " + id_rows);
        }
        //удаление позиции по id
        private async Task Delete(string arg, Chat chat)
        {
            int id = -1;
            try
            {
                id = int.Parse(arg);
            }
            catch
            {
                //
                throw new FormatException();
            }
            try
            {
                row r = await DataBase.getUserInfo(id);
                if (r.name != null)
                {
                    await DataBase.deleteRow(id);
                }
                else
                {
                    throw new FieldAccessException();
                }
            }
            catch
            {
                throw new FieldAccessException();
            }

            await Program._client.SendTextMessageAsync(chat, $"Запись успешно удалена.\r\n" +
                $"ID: " + id);
        }
        private async Task Sel(string arg, Chat chat)
        {
            int id = -1;
            try
            {
                id = int.Parse(arg);
            }
            catch
            {
                throw new FormatException();
            }
            row r = await DataBase.getUserInfo(id);
            if (r.name == null || r.phone == null || r.date == null)
            {
                throw new FieldAccessException();
            }
            await Program._client.SendTextMessageAsync(chat, $"ID: {arg}. \r\n" +
                $"Имя: {r.name}\r\n" +
                $"Телефон: {r.phone}\r\n" +
                $"Дата: {r.date.ToString("dd/MM/yyyy - HH:mm")}\r\n");
        }
        private async Task Help(Chat chat)
        {
            string prefix = Program.commandPrefix;
            await Program._client
                .SendTextMessageAsync(chat, $"Список доступных команд:\r\n" +
                $"{prefix}add <имя> <телефон> <дата> (новые строки перечисляются через ;) - добавить новую запись\r\n" +
                $"{prefix}del <ID> - удалить запись с заданным идентификатором\r\n" +
                $"{prefix}sel <ID> - информация о записи с заданным идентификатором\r\n" +
                $"{prefix}help - список всех доступных команд\r\n");
        }
    }
}
