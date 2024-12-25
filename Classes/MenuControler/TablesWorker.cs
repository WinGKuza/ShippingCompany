﻿using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using ShippingCompany.Database;

namespace ShippingCompany.Classes.MenuControler
{
    internal static class TablesWorker
    {
        public static void LoadTableFromDatabase(MainWindow mainWindow, string tableName)
        {
            // Очистка текущего содержимого
            mainWindow.MainContent.Children.Clear();

            // Создаем общий контейнер для DataGrid и кнопок
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
            string query = $"SELECT * FROM {tableName};";
            DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(query);

            // Устанавливаем сортировку по умолчанию
            DataView dataView = dataTable.DefaultView;
            dataView.Sort = "id ASC";

            // Создаем DataGrid
            DataGrid dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                ItemsSource = dataView, // Привязываем DataView с сортировкой
                IsReadOnly = !canEdit, // Редактирование разрешено только при e = true
                CanUserAddRows = false, // Отключаем стандартное добавление строк
                Margin = new Thickness(10)
            };

            DockPanel.SetDock(dataGrid, Dock.Top); // Размещаем DataGrid в верхней части
            mainPanel.Children.Add(dataGrid);

            // Добавляем колонку с кнопкой "Удалить", если есть права на удаление
            if (canDelete)
            {
                DataGridTemplateColumn deleteColumn = new DataGridTemplateColumn
                {
                    Header = "🗑 Удалить",
                    CellTemplate = CreateDeleteButtonTemplate(mainWindow, tableName, dataTable)
                };
                dataGrid.Columns.Add(deleteColumn);
            }

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

            // Контейнер для кнопок
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(10)
            };

            // Добавляем кнопку "Добавить", если есть права на запись
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
                        if (dataTable.Columns.Contains("id"))
                        {
                            newRow["id"] = dataTable.AsEnumerable().Select(row => row.Field<int>("id")).DefaultIfEmpty(0).Max() + 1;
                        }

                        dataTable.Rows.Add(newRow);
                        addButton.Content = "✔ Подтвердить";

                        // Блокируем кнопку "Сохранить", если она существует
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
                            SaveNewRowToDatabase(tableName, newRow);
                            MessageBox.Show("Строка успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            addButton.Content = "➕ Добавить";

                            // Разблокируем кнопку "Сохранить", если она существует
                            var saveButton = buttonPanel.Children
                                .OfType<Button>()
                                .FirstOrDefault(b => (string)b.Content == "💾 Сохранить");
                            if (saveButton != null)
                            {
                                saveButton.IsEnabled = true;
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

            // Добавляем кнопку "Сохранить", если есть права на редактирование
            if (canEdit)
            {
                Button saveButton = new Button
                {
                    Content = "💾 Сохранить",
                    Margin = new Thickness(5),
                    IsEnabled = true
                };

                saveButton.Click += (sender, args) => SaveTableChanges(mainWindow, tableName, dataTable);

                buttonPanel.Children.Add(saveButton);
            }

            DockPanel.SetDock(buttonPanel, Dock.Bottom); // Размещаем кнопки в нижней части
            mainPanel.Children.Add(buttonPanel);

            // Добавляем общий контейнер в MainContent
            mainWindow.MainContent.Children.Add(mainPanel);
        }


        private static DataTemplate CreateDeleteButtonTemplate(MainWindow mainWindow, string tableName, DataTable dataTable)
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
                            dataTable.Rows.Remove(row.Row);

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
                        else // Если строка существует
                        {
                            DeleteRowFromDatabase(mainWindow, tableName, row);
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

            DatabaseManager.Instance.ExecuteNonQuery(insertQuery, parameters.ToArray());
        }

        private static void SaveTableChanges(MainWindow mainWindow, string tableName, DataTable dataTable)
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

                    DatabaseManager.Instance.ExecuteNonQuery(updateQuery, parameters.ToArray());
                }
            }

            MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadTableFromDatabase(mainWindow, tableName);
        }

        private static string GenerateUpdateQuery(string tableName, DataRow row)
        {
            var setClause = string.Join(", ",
                row.Table.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName.ToLower() != "id") // Исключаем id
                    .Select(c => $"{c.ColumnName} = @{c.ColumnName}"));

            return $"UPDATE {tableName} SET {setClause} WHERE id = @id;";
        }

        private static void DeleteRowFromDatabase(MainWindow mainWindow, string tableName, DataRowView row)
        {
            string whereClause = string.Join(" AND ", row.Row.ItemArray.Select((value, index) =>
                $"{row.Row.Table.Columns[index].ColumnName} = @{row.Row.Table.Columns[index].ColumnName}"));

            string deleteQuery = $"DELETE FROM {tableName} WHERE {whereClause};";

            var parameters = row.Row.ItemArray.Select((value, index) =>
                new Npgsql.NpgsqlParameter($"@{row.Row.Table.Columns[index].ColumnName}", value)).ToArray();

            DatabaseManager.Instance.ExecuteNonQuery(deleteQuery, parameters);

            //MessageBox.Show("Запись успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadTableFromDatabase(mainWindow, tableName);
        }
    }
}
