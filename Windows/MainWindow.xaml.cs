using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using ShippingCompany.Database;

namespace ShippingCompany
{
    public partial class MainWindow
    {
        public string Username { get; private set; }

        public MainWindow(string username)
        {
            InitializeComponent();
            Username = username;
            LoadMenu();
        }

        private void LoadMenu()
        {
            // Получение ID пользователя
            int userId = GetUserId(Username);
            if (userId == -1)
            {
                MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Загрузка данных user_rights и menu
            var menuItems = GetUserMenuItems(userId);

            // Создание структуры меню
            var rootMenuItems = menuItems.Where(m => m.ParentId == 0).ToList();
            foreach (var rootItem in rootMenuItems)
            {
                var menuItem = CreateMenuItem(rootItem);

                // Добавление дочерних элементов
                AddChildMenuItems(menuItem, menuItems, rootItem.Id);

                // Добавляем элемент в Menu
                MainMenu.Items.Add(menuItem);
            }
        }

        private int GetUserId(string username)
        {
            string query = "SELECT id FROM app_user WHERE login = @login";
            var result = DatabaseManager.Instance.ExecuteScalar(query, new NpgsqlParameter("@login", username));
            return result != null ? Convert.ToInt32(result) : -1;
        }

        private List<MenuItemData> GetUserMenuItems(int userId)
        {
            string query = @"
                SELECT m.id, m.parent_id, m.name, m.function_name 
                FROM menu m
                JOIN user_rights ur ON ur.menu_id = m.id
                WHERE ur.app_user_id = @userId 
                  AND (ur.r = TRUE OR ur.w = TRUE OR ur.e = TRUE OR ur.d = TRUE)";

            DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(query, new NpgsqlParameter("@userId", userId));
            return (from DataRow row in dataTable.Rows
                    select new MenuItemData
                    {
                        Id = Convert.ToInt32(row["id"]),
                        ParentId = Convert.ToInt32(row["parent_id"]),
                        Name = row["name"].ToString(),
                        FunctionName = row["function_name"].ToString()
                    }).ToList();
        }

        private MenuItem CreateMenuItem(MenuItemData itemData)
        {
            var menuItem = new MenuItem
            {
                Header = itemData.Name,
                Tag = itemData.FunctionName
            };

            // Привязываем обработчик клика, если есть привязанная функция
            if (!string.IsNullOrEmpty(itemData.FunctionName))
            {
                menuItem.Click += (sender, args) => MenuItem_Click(itemData.FunctionName);
            }

            return menuItem;
        }

        private void AddChildMenuItems(MenuItem parentMenuItem, List<MenuItemData> menuItems, int parentId)
        {
            var childItems = menuItems.Where(m => m.ParentId == parentId).ToList();
            foreach (var childItem in childItems)
            {
                var menuItem = CreateMenuItem(childItem);
                parentMenuItem.Items.Add(menuItem);

                // Рекурсивное добавление дочерних элементов
                AddChildMenuItems(menuItem, menuItems, childItem.Id);
            }
        }

        private void MenuItem_Click(string functionName)
        {
            MessageBox.Show($"Функция: {functionName}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            // Здесь можно реализовать вызов соответствующих методов или переходов
        }

        // Класс для хранения данных об элементах меню
        private class MenuItemData
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Name { get; set; }
            public string FunctionName { get; set; }
        }
    }
}
