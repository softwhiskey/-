using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PrototypeTelegramBot
{
    internal static class DataBase
    {
        public static MySqlConnection connection;
        private static MySqlConnection busyConnection;
        //подключение к бд, инициализация
        public static async Task Init()
        {
            string server = Program.host;
            string database = Program.database;
            string uid = Program.uid;
            string password = Program.password;
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + "; PORT=3306;";
            connection = new MySqlConnection(connectionString);
            connection.ConnectionString = connectionString;
            connection.Open();
            busyConnection = new MySqlConnection(connectionString);
            busyConnection.ConnectionString = connectionString;
            busyConnection.Open();
            string query = $"SELECT * FROM applications";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.ExecuteNonQuery();
            List<row> newTempRows = new List<row>();
            MySqlDataReader reader = command.ExecuteReader();
            int tempRowCount = 0;
            while (reader.Read())
            {
                tempRowCount++;
            }
            reader.Close();
            tempRowsCount = tempRowCount;
        }
        //получение информации об пользователе по id
        public static async Task<row> getUserInfo(int id)
        {
            row data = new row();
            int index = 0;
            string query = "SELECT * FROM applications WHERE `id` = " + id;
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                data.date = dateTime.AddSeconds(double.Parse(reader[1].ToString())).ToLocalTime();
                data.name = reader[2].ToString();
                data.phone = reader[3].ToString();
            }
            reader.Close();
            return data;
        }
        //добавление параметра и возвращение айди (int) добавленной записи
        public static async Task<int> addRow(DateTime datetime, string name, string phone)
        {
            int index = 0; ;
            string query = "INSERT INTO applications VALUES (@id,@datetime,@name,@phone)";
            long unixTime = ((DateTimeOffset)datetime).ToUnixTimeSeconds();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", default);
            command.Parameters.AddWithValue("@datetime", unixTime);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@phone", phone);
            command.ExecuteNonQuery();
            //возврат айди {
            query = "SELECT id FROM applications WHERE datetime = @datetime AND name = @name AND phone = @phone";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@datetime", unixTime);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@phone", phone);
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                index = int.Parse(reader[0].ToString());
            }
            reader.Close();
            tempRowsCount++;
            return index;
            //}
        }
        //удаление параметра
        public static async Task deleteRow(int id)
        {
            string query = "DELETE FROM applications WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            tempRowsCount--;
        }
        private static System.Timers.Timer loopingTimer;
        private static int tempRowsCount = 0;
        //запуск парсера
        internal static Task StartTimer()
        {
            loopingTimer = new System.Timers.Timer()
            {
                Interval = 3000,
                AutoReset = true,
                Enabled = true
            };
            loopingTimer.Elapsed += OnTimerTicked;
            return Task.CompletedTask;
        }
        //парсер: считывание полей из БД и сравнивание их количества с предыдущим
        //если новое количество больше предудыщего (tempRowsCount), бот отправляет 
        //добавленные записи в канал
        private static async void OnTimerTicked(object sender, ElapsedEventArgs e)
        {
            string query = $"SELECT * FROM applications";
            MySqlCommand command = new MySqlCommand(query, busyConnection);
            command.ExecuteNonQuery();
            List<row> newTempRows = new List<row>();
            MySqlDataReader reader = command.ExecuteReader();
            int localRowsCount = 0;
            while (reader.Read())
            {
                localRowsCount++;
                row data = new row();
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                data.date = dateTime.AddSeconds(double.Parse(reader[1].ToString())).ToLocalTime();
                data.name = reader[2].ToString();
                data.phone = reader[3].ToString();
                newTempRows.Add(data);
            }
            reader.Close();
            if (localRowsCount > tempRowsCount)
            {
                //количество добавленных записей
                int spread = localRowsCount - tempRowsCount;
                tempRowsCount = localRowsCount;
                //row last_r = newTempRows.Last();
                int lastIndex = newTempRows.Count - 1;
                //отправка записей в канал
                for (int i = 0; i < spread; i++)
                {
                    await Program._client.SendTextMessageAsync(Program.lastChat,
                        "Добавлена запись из СУБД:\r\n" +
                        $"Имя: {newTempRows[lastIndex].name}\r\n" +
                        $"Телефон: {newTempRows[lastIndex].phone}\r\n" +
                        $"Дата: {newTempRows[lastIndex].date.ToString("dd/MM/yyyy - HH:mm")}\r\n");
                    lastIndex--;
                }
            }
        }
    }
}