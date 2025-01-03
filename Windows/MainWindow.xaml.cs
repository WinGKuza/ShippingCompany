﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Npgsql;
using ShippingCompany.Classes.Login;
using ShippingCompany.Database;

namespace ShippingCompany
{
    public partial class MainWindow
    {
        public string Username { get; private set; }
        public bool R { get; private set; } = false;
        public bool W { get; private set; } = false;
        public bool E { get; private set; } = false;
        public bool D { get; private set; } = false;

        public MainWindow(string username)
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri(@"C:\Users\danil\source\repos\DataBase\ShippingCompany\Icons\db.png"));
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
                var menuItem = CreateMenuItem(rootItem, menuItems.Any(m => m.ParentId == rootItem.Id), true);

                AddPermissionsFromDatabase(rootItem.FunctionName, Username); //!
                // Добавление дочерних элементов
                AddChildMenuItems(menuItem, menuItems, rootItem.Id);

                // Добавляем элемент в Menu
                MainMenu.Items.Add(menuItem);
            }
        }

        internal int GetUserId(string username)
        {
            string query = "SELECT id FROM app_user WHERE login = @login";
            var result = DatabaseManager.Instance.ExecuteScalar(query, new NpgsqlParameter("@login", username));
            return result != null ? Convert.ToInt32(result) : -1;
        }

        internal List<MenuItemData> GetUserMenuItems(int userId)
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

        private MenuItem CreateMenuItem(MenuItemData itemData, bool hasChildren, bool isRoot)
        {
            var menuItem = new MenuItem
            {
                Header = itemData.Name,
                Tag = itemData.FunctionName,
                Foreground = isRoot ? Brushes.White : Brushes.Black, // Цвет текста
                Background = isRoot ? new SolidColorBrush(Color.FromRgb(66, 133, 244)) : Brushes.White // Фон элемента 
            };

            // Добавляем обработчик только для элементов без дочерних элементов
            if (!hasChildren && !string.IsNullOrEmpty(itemData.FunctionName))
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
                bool hasChildren = menuItems.Any(m => m.ParentId == childItem.Id);
                var menuItem = CreateMenuItem(childItem, hasChildren, false);
                parentMenuItem.Items.Add(menuItem);

                AddPermissionsFromDatabase(childItem.FunctionName, Username);
                // Рекурсивное добавление дочерних элементов
                AddChildMenuItems(menuItem, menuItems, childItem.Id);
            }
        }

        // Метод для добавления прав доступа в GlobalDictionary из базы данных
        private void AddPermissionsFromDatabase(string functionName, string username)
        {
            if (string.IsNullOrEmpty(functionName) || string.IsNullOrEmpty(username)) return;

            // Запрос для получения прав доступа
            string query = @"
        SELECT ur.r, ur.w, ur.e, ur.d 
        FROM user_rights ur
        JOIN menu m ON ur.menu_id = m.id
        JOIN app_user au ON ur.app_user_id = au.id
        WHERE m.function_name = @functionName AND au.login = @username";

            var parameters = new[]
            {
        new NpgsqlParameter("@functionName", functionName),
        new NpgsqlParameter("@username", username)
    };

            var dataTable = DatabaseManager.Instance.ExecuteQuery(query, parameters);

            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                bool r = Convert.ToBoolean(row["r"]);
                bool w = Convert.ToBoolean(row["w"]);
                bool e = Convert.ToBoolean(row["e"]);
                bool d = Convert.ToBoolean(row["d"]);

                // Извлекаем имя метода из function_name (например, из "MainTables.Worker" получить "Worker")
                string methodName = functionName.Split('.').Last();

                // Добавляем данные в GlobalDictionary
                GlobalRightsDictionary.Set(ConvertCamelCaseToSnakeCase(methodName), (r, w, e, d));
            }
        }

        private static string ConvertCamelCaseToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Вставляем подчеркивание перед каждой заглавной буквой (кроме первой)
            return string.Concat(input.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? "_" + char.ToLower(x) : char.ToLower(x).ToString()));
        }


        private void MenuItem_Click(string functionName)
        {
            MenuActionExecutor.Execute(functionName, this);
        }

        // Класс для хранения данных об элементах меню
        public class MenuItemData
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Name { get; set; }
            public string FunctionName { get; set; }
        }
    }
}
