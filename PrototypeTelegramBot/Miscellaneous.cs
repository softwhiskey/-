using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototypeTelegramBot
{
    internal static class Miscellaneous
    {
        //перечисление комманд
        public static commandType returnEnumCommandType(string strValue)
        {
            switch (strValue)
            {
                case "add":
                    return commandType.add;
                case "del":
                    return commandType.del;
                case "sel":
                    return commandType.sel;
                case "help":
                    return commandType.help;
            }
            throw new TypeLoadException();
        }
        //разделение аргументов комманды (!add Alex 791655... etc.)
        public static List<row> separateRows(string arg)
        {
            List<row> temp_r = new List<row>();
            if (arg.Contains(";"))
            {
                arg = arg.Remove(0, 5);
                List<string> temp = arg.Split(';').ToList();
                if (string.IsNullOrWhiteSpace(temp.Last())) temp.Remove(temp.Last());
                CultureInfo ci = new CultureInfo("RU-ru");
                foreach (string s in temp)
                {
                    //извлечение каждого из 3х значений (имя, телефон, дата) 
                    //и передача их в экземпляр `r` (структуры row)
                    row r = new row();
                    string temp_v = s.TrimStart();
                    r.name = temp_v;
                    r.name = r.name.Remove(r.name.IndexOf(" "));
                    r.phone = temp_v.Remove(0, r.name.Length + 1);
                    string temp_date = r.phone;
                    r.phone = r.phone.Remove(r.phone.IndexOf(" "));
                    temp_date = temp_date.Remove(0, r.phone.Length + 1);
                    temp_date = temp_date.TrimEnd(';', ' ');
                    r.date = DateTime.Parse(temp_date, ci);
                    temp_r.Add(r);
                }
                return temp_r;
            }
            else
            {
                //если входная строка одна
                row r = new row();
                arg = arg.Remove(0, 5);
                string temp_v = arg.TrimStart();
                r.name = temp_v;
                r.name = r.name.Remove(r.name.IndexOf(" "));
                r.phone = temp_v.Remove(0, r.name.Length + 1);
                string temp_date = r.phone;
                r.phone = r.phone.Remove(r.phone.IndexOf(" "));
                temp_date = temp_date.Remove(0, r.phone.Length + 1);
                temp_date = temp_date.TrimEnd(';', ' ');
                r.date = DateTime.Parse(temp_date);
                temp_r.Add(r);
                return temp_r;
            }
        }
        public static string getId(string arg)
        {
            return arg.Substring(arg.IndexOf(" ") + 1);
        }
    }
    //Структура для быстрой визуализации и работы с информацией о пользователях из БД
    struct row
    {
        public string name { get; set; }
        public string phone { get; set; }
        public DateTime date { get; set; }
        public row (string name, string phone, DateTime date)
        {
            this.name = name;
            this.phone = phone;
            this.date = date;
        }
    }
}
