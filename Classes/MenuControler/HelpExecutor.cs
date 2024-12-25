using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Documents;
using System.Collections.Generic;
using System;
using Npgsql;
using System.Linq;
using static ShippingCompany.MainWindow;

namespace ShippingCompany.Classes
{
    public static class HelpExecutor
    {
        public static void ClearContent(MainWindow mainWindow)
        {
            // Очистка текущего содержимого окна
            mainWindow.MainContent.Children.Clear();
        }

        public static void ShowProjectInfo(MainWindow mainWindow, string menuItemName)
        {
            ClearContent(mainWindow);
            mainWindow.Title = menuItemName;
            // Создаем контейнер для текста
            TextBlock infoTextBlock = new TextBlock
            {
                TextAlignment = TextAlignment.Justify,
                Text = "Работа выполнена студентом 3-его курса АВТФ НГТУ группы АВТ-214 Кузнецовым Данилом в рамках программы дисциплины " +
                "\"Базы данных\" в осеннем семестре 2024 года под руководством ассистента кафедры Автоматизированных Систем Управления Антонянца Егора Николаевича.",
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            // Ссылка на GitHub
            StackPanel linkPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            Image githubLogo = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png")),
                Width = 20,
                Height = 20,
                Margin = new Thickness(0, 0, 15, 0)
            };

            Hyperlink repoLink = new Hyperlink
            {
                NavigateUri = new System.Uri("https://github.com/WinGKuza/ShippingCompany")
            };
            repoLink.Inlines.Add(new Run("GitHub") { FontSize = 16 });
            repoLink.RequestNavigate += (sender, e) =>
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            };

            TextBlock repoTextBlock = new TextBlock();
            repoTextBlock.Inlines.Add(repoLink);

            linkPanel.Children.Add(githubLogo);
            linkPanel.Children.Add(repoTextBlock);

            // Добавляем элементы в интерфейс
            StackPanel mainPanel = new StackPanel();
            mainPanel.Children.Add(infoTextBlock);
            mainPanel.Children.Add(linkPanel);

            mainWindow.MainContent.Children.Add(mainPanel);
        }

        public static void ShowProjectContents(MainWindow mainWindow, Func<int, List<MainWindow.MenuItemData>> getUserMenuItems, string menuItemName)
        {
            ClearContent(mainWindow);
            mainWindow.Title = menuItemName;
            // Создаем контейнер для содержания
            StackPanel contentsPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10)
            };

            // Получение данных меню
            int userId = mainWindow.GetUserId(mainWindow.Username);
            var menuItems = getUserMenuItems(userId);

            if (menuItems == null || menuItems.Count == 0)
            {
                MessageBox.Show("Меню пользователя не найдено или пусто.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создание структуры меню
            var rootMenuItems = menuItems.Where(m => m.ParentId == 0).ToList();

            foreach (var rootItem in rootMenuItems)
            {
                StackPanel rootPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                Hyperlink rootLink = new Hyperlink
                {
                    NavigateUri = null
                };
                rootLink.Inlines.Add(new Run(rootItem.Name));
                rootLink.Click += (sender, e) =>
                {
                    MenuActionExecutor.Execute(rootItem.FunctionName, mainWindow);
                };

                TextBlock rootTextBlock = new TextBlock
                {
                    Margin = new Thickness(0, 5, 0, 5)
                };
                rootTextBlock.Inlines.Add(rootLink);

                rootPanel.Children.Add(rootTextBlock);

                // Добавляем дочерние элементы
                var childItems = menuItems.Where(m => m.ParentId == rootItem.Id).ToList();
                if (childItems.Any())
                {
                    StackPanel childPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Margin = new Thickness(20, 0, 0, 0)
                    };

                    foreach (var childItem in childItems)
                    {
                        Hyperlink childLink = new Hyperlink
                        {
                            NavigateUri = null
                        };
                        childLink.Inlines.Add(new Run(childItem.Name));
                        childLink.Click += (sender, e) =>
                        {
                            MenuActionExecutor.Execute(childItem.FunctionName, mainWindow);
                        };

                        TextBlock childTextBlock = new TextBlock
                        {
                            Margin = new Thickness(0, 5, 0, 5)
                        };
                        childTextBlock.Inlines.Add(childLink);

                        childPanel.Children.Add(childTextBlock);
                    }

                    rootPanel.Children.Add(childPanel);
                }

                contentsPanel.Children.Add(rootPanel);
            }

            mainWindow.MainContent.Children.Add(contentsPanel);
        }
    }
}
