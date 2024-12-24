using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Npgsql;

namespace ShippingCompany.Database
{
    public class DatabaseManager
    {
        private static DatabaseManager _instance;
        private static readonly object _lock = new object();

        private readonly string _connectionString;

        // Приватный конструктор, чтобы исключить создание объектов напрямую
        private DatabaseManager(string host, string database, string username, string password, int port = 5432)
        {
            _connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock) // Потокобезопасность
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseManager("localhost", "postgres", "postgres", "postgres");
                        }
                    }
                }
                return _instance;
            }
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // Метод проверки подключения к базе данных
        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open(); // Попытка открыть соединение
                    return connection.State == ConnectionState.Open; // Возвращаем true, если соединение успешно
                }
            }
            catch
            {
                return false; // Если произошла ошибка, возвращаем false
            }
        }

        public DataTable ExecuteQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public int ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string query, params NpgsqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteScalar();
                }
            }
        }
    }
}

