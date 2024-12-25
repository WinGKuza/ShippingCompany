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
        public static void LoadTableFromDatabase(MainWindow mainWindow, string tableName)
        {
            // Очистка текущего содержимого
            mainWindow.MainContent.Children.Clear();

            // Проверяем права на редактирование (e) и удаление (d)
            var permissions = GlobalRightsDictionary.Get(tableName);
            bool canEdit = permissions.e;
            bool canDelete = permissions.d;

            // Выполнение SQL-запроса
            string query = $"SELECT * FROM {tableName};";
            DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(query);

            // Создаем DataGrid
            DataGrid dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                ItemsSource = dataTable.DefaultView,
                IsReadOnly = !canEdit, // Редактирование разрешено только при e = true
                CanUserAddRows = false // Отключаем добавление новых строк
            };

            // Добавляем остальные колонки с сортировкой
            foreach (DataColumn column in dataTable.Columns)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn
                {
                    Header = column.ColumnName,
                    Binding = new System.Windows.Data.Binding($"[{column.ColumnName}]"),
                    IsReadOnly = column.ColumnName.ToLower() == "id", // id всегда только для чтения
                    CanUserSort = true // Включаем сортировку
                };

                // Связываем сортировку с именем столбца
                textColumn.SortMemberPath = column.ColumnName;
                dataGrid.Columns.Add(textColumn);
            }

            // Добавляем колонку с кнопками удаления, если есть права
            if (canDelete)
            {
                DataGridTemplateColumn deleteColumn = new DataGridTemplateColumn
                {
                    Header = "🗑 Удалить",
                    CellTemplate = CreateDeleteButtonTemplate(mainWindow, tableName, dataTable)
                };
                dataGrid.Columns.Add(deleteColumn);
            }

            // Добавляем DataGrid в MainWindow
            mainWindow.MainContent.Children.Add(dataGrid);

            // Добавляем кнопку сохранения, если есть права на редактирование
            if (canEdit)
            {
                Button saveButton = new Button
                {
                    Content = "💾 Сохранить",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10)
                };
                saveButton.Click += (sender, args) => SaveTableChanges(mainWindow, tableName, dataTable);
                mainWindow.MainContent.Children.Add(saveButton);
            }
        }




        private static DataTemplate CreateDeleteButtonTemplate(MainWindow mainWindow, string tableName, DataTable dataTable)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(Button.ContentProperty, "🗑 Удалить"); // Добавляем смайлик корзины
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

        private static void SaveTableChanges(MainWindow mainWindow, string tableName, DataTable dataTable)
        {
            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row.RowState == DataRowState.Modified) // Проверяем только измененные строки
                    {
                        string updateQuery = GenerateUpdateQuery(tableName, row);
                        var parameters = row.Table.Columns.Cast<DataColumn>()
                            .Where(c => c.ColumnName.ToLower() != "id") // Исключаем id
                            .Select(c => new Npgsql.NpgsqlParameter($"@{c.ColumnName}", row[c.ColumnName]))
                            .ToList();

                        parameters.Add(new Npgsql.NpgsqlParameter("@id", row["id"]));

                        DatabaseManager.Instance.ExecuteNonQuery(updateQuery, parameters.ToArray());
                    }
                }

                MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Перезагружаем таблицу для отображения актуальных данных
                LoadTableFromDatabase(mainWindow, tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GenerateUpdateQuery(string tableName, DataRow row)
        {
            var setClause = string.Join(", ",
                row.Table.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName.ToLower() != "id") // Исключаем id
                    .Select(c => $"{c.ColumnName} = @{c.ColumnName}"));

            return $"UPDATE {tableName} SET {setClause} WHERE id = @id;";
        }
    }
}
