using System;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ShippingCompany.Database;

namespace ShippingCompany.Classes
{
    public static class SqlQueryExecutor
    {
        public static void ShowSqlQueryInterface(MainWindow mainWindow, string menuItemName, string initialQuery = "")
        {
            // Очистка текущего содержимого
            mainWindow.MainContent.Children.Clear();
            mainWindow.Title = menuItemName;

            // Создаем контейнер для интерфейса
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10)
            };

            // Поле для ввода SQL-запроса
            TextBox queryTextBox = new TextBox
            {
                Text = initialQuery,
                Height = 100,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Кнопка для выполнения запроса
            Button executeButton = new Button
            {
                Content = "Исполнить запрос",
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Кнопка для экспорта в CSV
            Button exportButton = new Button
            {
                Content = "Экспортировать в CSV",
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Visibility = Visibility.Collapsed // Скрыта по умолчанию
            };

            // Текстовое поле для отображения результатов
            TextBlock resultTextBlock = new TextBlock
            {
                Text = string.Empty,
                Foreground = Brushes.Green,
                Margin = new Thickness(0, 10, 0, 0),
                Visibility = Visibility.Collapsed
            };

            // DataGrid для отображения SELECT-запросов
            DataGrid dataGrid = new DataGrid
            {
                AutoGenerateColumns = true,
                CanUserAddRows = false,
                IsReadOnly = true,
                Margin = new Thickness(0, 10, 0, 0),
                Visibility = Visibility.Collapsed
            };

            // Создаем горизонтальную панель для кнопок
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Добавляем кнопки в горизонтальную панель
            buttonPanel.Children.Add(executeButton);
            buttonPanel.Children.Add(exportButton);

            // Обработчик нажатия кнопки "Исполнить запрос"
            executeButton.Click += (sender, args) =>
            {
                string query = queryTextBox.Text.Trim();

                if (string.IsNullOrEmpty(query))
                {
                    MessageBox.Show("Введите SQL-запрос.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    if (query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        // Выполнение SELECT-запроса
                        DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(query);

                        if (dataTable.Rows.Count > 0)
                        {
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            dataGrid.Visibility = Visibility.Visible;
                            resultTextBlock.Visibility = Visibility.Collapsed;
                            exportButton.Visibility = Visibility.Visible;
                            exportButton.Click += (exportSender, exportArgs) => ExportTableToCsv(dataTable, "export.csv");
                        }
                        else
                        {
                            dataGrid.Visibility = Visibility.Collapsed;
                            resultTextBlock.Text = "Запрос выполнен успешно, но данные отсутствуют.";
                            resultTextBlock.Visibility = Visibility.Visible;
                            exportButton.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        // Выполнение не-SELECT-запроса
                        DatabaseManager.Instance.ExecuteNonQuery(query);
                        dataGrid.Visibility = Visibility.Collapsed;
                        resultTextBlock.Text = "Запрос выполнен успешно.";
                        resultTextBlock.Visibility = Visibility.Visible;
                        exportButton.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            // Добавляем элементы в контейнер
            panel.Children.Add(queryTextBox);
            panel.Children.Add(buttonPanel); // Добавляем горизонтальную панель с кнопками
            panel.Children.Add(dataGrid);
            panel.Children.Add(resultTextBlock);

            // Добавляем контейнер в MainContent
            mainWindow.MainContent.Children.Add(panel);
        }

        private static void ExportTableToCsv(DataTable dataTable, string filePath)
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Формируем содержимое CSV
                StringBuilder csvContent = new StringBuilder();

                // Заголовки столбцов
                string[] columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                csvContent.AppendLine(string.Join(",", columnNames));

                // Строки данных
                foreach (DataRow row in dataTable.Rows)
                {
                    string[] fields = row.ItemArray.Select(field => field.ToString().Replace(",", "\\,")).ToArray();
                    csvContent.AppendLine(string.Join(",", fields));
                }

                // Запись в файл
                File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);

                MessageBox.Show($"Данные успешно экспортированы в файл {filePath}.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
