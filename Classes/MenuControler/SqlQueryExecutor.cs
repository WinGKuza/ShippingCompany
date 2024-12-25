using System;
using System.Data;
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
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Center
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
                        }
                        else
                        {
                            dataGrid.Visibility = Visibility.Collapsed;
                            resultTextBlock.Text = "Запрос выполнен успешно, но данные отсутствуют.";
                            resultTextBlock.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        // Выполнение не-SELECT-запроса
                        DatabaseManager.Instance.ExecuteNonQuery(query);
                        dataGrid.Visibility = Visibility.Collapsed;
                        resultTextBlock.Text = "Запрос выполнен успешно.";
                        resultTextBlock.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            // Добавляем элементы в контейнер
            panel.Children.Add(queryTextBox);
            panel.Children.Add(executeButton);
            panel.Children.Add(dataGrid);
            panel.Children.Add(resultTextBlock);

            // Добавляем контейнер в MainContent
            mainWindow.MainContent.Children.Add(panel);
        }
    }
}
