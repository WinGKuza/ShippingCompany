using System;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using ShippingCompany.Database;

namespace ShippingCompany.Classes.MenuControler
{
    internal static class TablesWorker
    {
        /// <summary>
        /// Универсальный метод для загрузки данных из указанной таблицы базы данных с возможностью удаления.
        /// </summary>
        /// <param name="mainWindow">Ссылка на MainWindow для добавления содержимого.</param>
        /// <param name="tableName">Название таблицы базы данных.</param>
        public static void LoadTableFromDatabase(MainWindow mainWindow, string tableName)
        {
            // Очистка текущего содержимого
            mainWindow.MainContent.Children.Clear();

            // Проверяем права на удаление (d)
            var permissions = GlobalRightsDictionary.Get(tableName);
            bool canDelete = permissions.d;

            // Выполнение SQL-запроса
            string query = $"SELECT * FROM {tableName};";
            DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(query);

            // Создаем DataGrid
            DataGrid dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                ItemsSource = dataTable.DefaultView
            };

            // Добавляем колонку с кнопками удаления, если есть права
            if (canDelete)
            {
                DataGridTemplateColumn deleteColumn = new DataGridTemplateColumn
                {
                    Header = "Удалить",
                    CellTemplate = CreateDeleteButtonTemplate(mainWindow, tableName, dataTable)
                };
                dataGrid.Columns.Add(deleteColumn);
            }

            // Добавляем остальные колонки
            foreach (DataColumn column in dataTable.Columns)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn
                {
                    Header = column.ColumnName,
                    Binding = new System.Windows.Data.Binding($"[{column.ColumnName}]")
                };
                dataGrid.Columns.Add(textColumn);
            }

            // Добавляем DataGrid в MainWindow
            mainWindow.MainContent.Children.Add(dataGrid);
        }

        /// <summary>
        /// Создает шаблон для кнопки удаления в колонке DataGrid.
        /// </summary>
        /// <param name="tableName">Название таблицы базы данных.</param>
        /// <param name="dataTable">Текущая таблица данных.</param>
        /// <returns>DataTemplate с кнопкой удаления.</returns>
        private static DataTemplate CreateDeleteButtonTemplate(MainWindow mainWindow, string tableName, DataTable dataTable)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(Button.ContentProperty, "Удалить");
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, args) =>
            {
                // Получаем строку из кнопки
                var button = sender as Button;
                if (button != null)
                {
                    var row = button.DataContext as DataRowView;
                    if (row != null)
                    {
                        DeleteRowFromDatabase(mainWindow, tableName, row);
                    }
                }
            }));
            template.VisualTree = buttonFactory;
            return template;
        }

        /// <summary>
        /// Удаляет строку из базы данных.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <param name="row">Строка данных для удаления.</param>
        /// <summary>
        /// Удаляет строку из базы данных и обновляет отображение таблицы.
        /// </summary>
        /// <param name="tableName">Название таблицы.</param>
        /// <param name="row">Строка данных для удаления.</param>
        /// <param name="mainWindow">Ссылка на MainWindow для обновления содержимого.</param>
        private static void DeleteRowFromDatabase(MainWindow mainWindow, string tableName, DataRowView row)
        {
            try
            {
                // Генерация SQL-запроса для удаления строки
                string whereClause = string.Join(" AND ", row.Row.ItemArray.Select((value, index) =>
                    $"{row.Row.Table.Columns[index].ColumnName} = @{row.Row.Table.Columns[index].ColumnName}"));

                string deleteQuery = $"DELETE FROM {tableName} WHERE {whereClause};";

                var parameters = row.Row.ItemArray.Select((value, index) =>
                    new Npgsql.NpgsqlParameter($"@{row.Row.Table.Columns[index].ColumnName}", value)).ToArray();

                int rowsAffected = DatabaseManager.Instance.ExecuteNonQuery(deleteQuery, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Повторная загрузка данных из таблицы
                    LoadTableFromDatabase(mainWindow, tableName);
                }
                else
                {
                    MessageBox.Show("Не удалось удалить запись.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
