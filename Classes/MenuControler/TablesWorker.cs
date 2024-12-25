using ShippingCompany.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ShippingCompany.Classes.MenuControler
{
    internal static class TablesWorker
    {
        /// <summary>
        /// Метод для очистки содержимого MainWindow.
        /// </summary>
        /// <param name="mainWindow">Ссылка на MainWindow для очистки содержимого.</param>
        public static void ClearMainWindowContent(MainWindow mainWindow)
        {
            mainWindow.MainContent.Children.Clear();
        }

        /// <summary>
        /// Универсальный метод для загрузки данных из указанной таблицы базы данных.
        /// </summary>
        /// <param name="mainWindow">Ссылка на MainWindow для добавления содержимого.</param>
        /// <param name="tableName">Название таблицы базы данных.</param>
        public static void LoadTableFromDatabase(MainWindow mainWindow, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                MessageBox.Show("Имя таблицы не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Очистка текущего содержимого
                mainWindow.MainContent.Children.Clear();

                // Выполнение SQL-запроса
                string query = $"SELECT * FROM {tableName};";
                DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(query);

                // Преобразование DataTable в DataGrid
                DataGrid dataGrid = new DataGrid
                {
                    ItemsSource = dataTable.DefaultView
                };

                // Добавление DataGrid в MainWindow
                mainWindow.MainContent.Children.Add(dataGrid);
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
            {
                MessageBox.Show($"Таблица '{tableName}' не существует в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке таблицы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Метод для создания и отображения структуры таблицы из БД в MainWindow.
        /// </summary>
        /// <param name="mainWindow">Ссылка на MainWindow для добавления содержимого.</param>
        /// <param name="tableName">Название таблицы базы данных.</param>
        public static void CreateCustomTable(MainWindow mainWindow, string tableName)
        {
            // Очистка текущего содержимого
            mainWindow.MainContent.Children.Clear();

            // Запрос для получения структуры таблицы
            string query = $"SELECT * FROM {tableName} LIMIT 0;";
            DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(query);

            // Проверяем, если таблица имеет столбцы
            if (dataTable.Columns.Count > 0)
            {
                // Преобразование DataTable в DataGrid
                DataGrid dataGrid = new DataGrid
                {
                    ItemsSource = dataTable.DefaultView
                };

                // Добавление DataGrid в MainWindow
                mainWindow.MainContent.Children.Add(dataGrid);
            }
            else
            {
                // Если структура таблицы пуста
                TextBlock textBlock = new TextBlock
                {
                    Text = "Таблица не содержит данных или не найдена.",
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };

                mainWindow.MainContent.Children.Add(textBlock);
            }
        }
    }
}
