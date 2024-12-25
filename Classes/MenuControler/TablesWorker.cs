using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Npgsql;
using ShippingCompany.Database;

namespace ShippingCompany.Classes.MenuControler
{
    internal static class TablesWorker
    {
        public static void LoadTableFromDatabase(MainWindow mainWindow, string tableName, string menuItemName)
        {
            // Устанавливаем заголовок окна равным названию элемента меню
            mainWindow.Title = menuItemName;

            // Очистка текущего содержимого
            mainWindow.MainContent.Children.Clear();

            // Создаем общий контейнер для DataGrid, строки поиска и кнопок
            DockPanel mainPanel = new DockPanel();

            // Проверяем права доступа
            var permissions = GlobalRightsDictionary.Get(tableName);
            bool canRead = permissions.r;
            bool canWrite = permissions.w;
            bool canEdit = permissions.e;
            bool canDelete = permissions.d;

            // Если нет прав на чтение, выходим
            if (!canRead)
            {
                MessageBox.Show("У вас нет прав на просмотр данной таблицы.", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Выполнение SQL-запроса
            string baseQuery = $"SELECT * FROM {tableName};";
            DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(baseQuery);

            // Проверяем, что данные были получены
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                MessageBox.Show("Данные не найдены или запрос вернул пустой результат.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Устанавливаем сортировку по умолчанию
            DataView dataView = dataTable.DefaultView;
            dataView.Sort = "id ASC";

            // Объявляем DataGrid
            DataGrid dataGrid = new DataGrid
            {
                AutoGenerateColumns = true,
                ItemsSource = dataView, // Привязываем DataView с сортировкой
                IsReadOnly = !canEdit, // Редактирование разрешено только при e = true
                CanUserAddRows = false, // Отключаем стандартное добавление строк
                Margin = new Thickness(10)
            };

            // Добавляем колонку с кнопкой "Удалить" при открытии таблицы
            if (canDelete)
            {
                DataGridTemplateColumn deleteColumn = new DataGridTemplateColumn
                {
                    Header = "🗑 Удалить",
                    CellTemplate = CreateDeleteButtonTemplate(mainWindow, tableName, dataTable, menuItemName)
                };

                dataGrid.Columns.Insert(0, deleteColumn); // Добавляем колонку удаления в начало
            }

            // Добавляем строку поиска
            StackPanel searchPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10)
            };

            TextBox searchTextBox = new TextBox
            {
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 5, 0)
            };

            Button searchButton = new Button
            {
                Content = "🔍",
                Width = 50,
                HorizontalAlignment = HorizontalAlignment.Left,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(66, 133, 244)) 
            };

            searchButton.Click += (sender, args) =>
            {
                string searchText = searchTextBox.Text.Trim();

                string query;
                if (string.IsNullOrEmpty(searchText))
                {
                    query = baseQuery;
                }
                else
                {
                    query = $"SELECT * FROM {tableName} WHERE " +
                            string.Join(" OR ", dataTable.Columns
                                .Cast<DataColumn>()
                                .Select(c => $"CAST({c.ColumnName} AS TEXT) ILIKE '%{searchText}%'"));
                }

                try
                {
                    DataTable searchResults = DatabaseManager.Instance.ExecuteQuery(query);
                    dataTable.Clear();
                    foreach (DataRow row in searchResults.Rows)
                    {
                        dataTable.ImportRow(row);
                    }

                    dataGrid.ItemsSource = dataTable.DefaultView;

                    if (canDelete && dataGrid.Columns.All(col => col.Header?.ToString() != "🗑 Удалить"))
                    {
                        DataGridTemplateColumn deleteColumn = new DataGridTemplateColumn
                        {
                            Header = "🗑 Удалить",
                            CellTemplate = CreateDeleteButtonTemplate(mainWindow, tableName, dataTable, menuItemName)
                        };

                        dataGrid.Columns.Insert(0, deleteColumn);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выполнении поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            searchPanel.Children.Add(searchTextBox);
            searchPanel.Children.Add(searchButton);
            DockPanel.SetDock(searchPanel, Dock.Top);
            mainPanel.Children.Add(searchPanel);

            DockPanel.SetDock(dataGrid, Dock.Top);
            mainPanel.Children.Add(dataGrid);

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(10)
            };

            if (canWrite)
            {
                Button addButton = new Button
                {
                    Content = "➕ Добавить",
                    Margin = new Thickness(5)
                };

                addButton.Click += (sender, args) =>
                {
                    if ((string)addButton.Content == "➕ Добавить")
                    {
                        DataRow newRow = dataTable.NewRow();
                        string getMaxIdQuery = $"SELECT MAX(id) FROM {tableName};";
                        try
                        {
                            object result = DatabaseManager.Instance.ExecuteScalar(getMaxIdQuery);
                            int maxId = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                            newRow["id"] = maxId + 1;
                            foreach (DataColumn column in dataTable.Columns)
                            {
                                if (column.ColumnName.ToLower() != "id")
                                {
                                    if (column.DataType == typeof(string))
                                    {
                                        newRow[column.ColumnName] = "Слово";
                                    }
                                    else if (column.DataType == typeof(int) || column.DataType == typeof(long))
                                    {
                                        newRow[column.ColumnName] = 1;
                                    }
                                    else if (column.DataType == typeof(DateTime))
                                    {
                                        newRow[column.ColumnName] = DateTime.Now;
                                    }
                                    else if (column.DataType == typeof(double) || column.DataType == typeof(float))
                                    {
                                        newRow[column.ColumnName] = 0.1;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при вычислении нового ID: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        dataTable.Rows.Add(newRow);
                        addButton.Content = "✔ Подтвердить";

                        var saveButton = buttonPanel.Children
                            .OfType<Button>()
                            .FirstOrDefault(b => (string)b.Content == "💾 Сохранить");
                        if (saveButton != null)
                        {
                            saveButton.IsEnabled = false;
                        }
                    }
                    else if ((string)addButton.Content == "✔ Подтвердить")
                    {
                        try
                        {
                            DataRow newRow = dataTable.Rows[dataTable.Rows.Count - 1];
                            bool f = true;
                            try
                            {
                                SaveNewRowToDatabase(tableName, newRow);
                            }
                            catch (Exception ex)
                            {
                                f = false;
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            if (f)
                            {
                                MessageBox.Show("Строка успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                addButton.Content = "➕ Добавить";

                                var saveButton = buttonPanel.Children
                                    .OfType<Button>()
                                    .FirstOrDefault(b => (string)b.Content == "💾 Сохранить");
                                if (saveButton != null)
                                {
                                    saveButton.IsEnabled = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при добавлении строки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                };

                buttonPanel.Children.Add(addButton);
            }

            if (canEdit)
            {
                Button saveButton = new Button
                {
                    Content = "💾 Сохранить",
                    Margin = new Thickness(5),
                    IsEnabled = true
                };

                saveButton.Click += (sender, args) => SaveTableChanges(mainWindow, tableName, dataTable, menuItemName);
                buttonPanel.Children.Add(saveButton);
            }

            DockPanel.SetDock(buttonPanel, Dock.Bottom);
            mainPanel.Children.Add(buttonPanel);

            mainWindow.MainContent.Children.Add(mainPanel);
        }







        private static DataTemplate CreateDeleteButtonTemplate(MainWindow mainWindow, string tableName, DataTable dataTable, string menuItemName)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(Button.ContentProperty, "🗑 Удалить");
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, args) =>
            {
                var button = sender as Button;
                if (button != null)
                {
                    var row = button.DataContext as DataRowView;
                    if (row != null)
                    {
                        if (row.IsNew) // Если строка новая
                        {
                            // Удаляем строку из DataTable
                            bool f = true;
                            try
                            {
                                dataTable.Rows.Remove(row.Row);
                            }
                            catch (Exception ex)
                            {
                                f = false;
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            if (f)
                            {
                                // Меняем текст кнопки "Подтвердить" обратно на "Добавить"
                                var addButton = mainWindow.MainContent.Children
                                    .OfType<Button>()
                                    .FirstOrDefault(b => (string)b.Content == "✔ Подтвердить");
                                if (addButton != null)
                                {
                                    addButton.Content = "➕ Добавить";
                                }
                                // Разблокируем кнопку "Сохранить"
                                var saveButton = mainWindow.MainContent.Children
                                    .OfType<Button>()
                                    .FirstOrDefault(b => (string)b.Content == "💾 Сохранить");
                                if (saveButton != null)
                                {
                                    saveButton.IsEnabled = true;
                                }
                            }
                        }
                        else // Если строка существует
                        {
                            try
                            {
                                DeleteRowFromDatabase(mainWindow, tableName, row, menuItemName);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }));
            template.VisualTree = buttonFactory;
            return template;
        }

        private static void SaveNewRowToDatabase(string tableName, DataRow newRow)
        {
            string columnNames = string.Join(", ", newRow.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName.ToLower() != "id").Select(c => c.ColumnName));
            string columnValues = string.Join(", ", newRow.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName.ToLower() != "id").Select(c => $"@{c.ColumnName}"));

            string insertQuery = $"INSERT INTO {tableName} (id, {columnNames}) VALUES (@id, {columnValues});";

            var parameters = newRow.Table.Columns.Cast<DataColumn>()
                .Select(c => new Npgsql.NpgsqlParameter($"@{c.ColumnName}", newRow[c.ColumnName]))
                .ToList();

            parameters.Add(new Npgsql.NpgsqlParameter("@id", newRow["id"]));

            try
            {
                DatabaseManager.Instance.ExecuteNonQuery(insertQuery, parameters.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void SaveTableChanges(MainWindow mainWindow, string tableName, DataTable dataTable, string menuItemName)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if (row.RowState == DataRowState.Modified) // Измененные строки
                {
                    string updateQuery = GenerateUpdateQuery(tableName, row);
                    var parameters = row.Table.Columns.Cast<DataColumn>()
                        .Where(c => c.ColumnName.ToLower() != "id") // Исключаем id
                        .Select(c => new Npgsql.NpgsqlParameter($"@{c.ColumnName}", row[c.ColumnName]))
                        .ToList();

                    parameters.Add(new Npgsql.NpgsqlParameter("@id", row["id"]));

                    try
                    {
                        DatabaseManager.Instance.ExecuteNonQuery(updateQuery, parameters.ToArray());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadTableFromDatabase(mainWindow, tableName, menuItemName);
        }

        private static string GenerateUpdateQuery(string tableName, DataRow row)
        {
            var setClause = string.Join(", ",
                row.Table.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName.ToLower() != "id") // Исключаем id
                    .Select(c => $"{c.ColumnName} = @{c.ColumnName}"));

            return $"UPDATE {tableName} SET {setClause} WHERE id = @id;";
        }

        private static void DeleteRowFromDatabase(MainWindow mainWindow, string tableName, DataRowView row, string menuItemName)
        {
            string whereClause = string.Join(" AND ", row.Row.ItemArray.Select((value, index) =>
                $"{row.Row.Table.Columns[index].ColumnName} = @{row.Row.Table.Columns[index].ColumnName}"));

            string deleteQuery = $"DELETE FROM {tableName} WHERE {whereClause};";

            var parameters = row.Row.ItemArray.Select((value, index) =>
                new Npgsql.NpgsqlParameter($"@{row.Row.Table.Columns[index].ColumnName}", value)).ToArray();

            try
            {
                DatabaseManager.Instance.ExecuteNonQuery(deleteQuery, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadTableFromDatabase(mainWindow, tableName, menuItemName);
        }
    }
}
