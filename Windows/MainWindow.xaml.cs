using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using ShippingCompany.Classes.MenuControler;
using ShippingCompany.Database;

namespace ShippingCompany
{
    public partial class MainWindow
    {
        public string Username { get; private set; }
        public bool CanRead { get; private set; } = false;
        public bool CanWrite { get; private set; } = false;
        public bool CanEdit { get; private set; } = false;
        public bool CanDelete { get; private set; } = false;

        public MainWindow(string username)
        {
            InitializeComponent();
            Username = username;
            LoadMenu();
            LoadUserRights();
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
                var menuItem = CreateMenuItem(rootItem, menuItems.Any(m => m.ParentId == rootItem.Id));

                // Добавление дочерних элементов
                AddChildMenuItems(menuItem, menuItems, rootItem.Id);

                // Добавляем элемент в Menu
                MainMenu.Items.Add(menuItem);
            }
        }

        private void LoadUserRights()
        {
            // Получение ID пользователя из таблицы app_user
            string getIdQuery = "SELECT id FROM app_user WHERE login = @login";
            var userId = DatabaseManager.Instance.ExecuteScalar(getIdQuery, new NpgsqlParameter("@login", Username));

            if (userId == null)
            {
                MessageBox.Show("Не удалось получить ID пользователя. Проверьте данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Получение прав доступа из таблицы user_rights
            string getRightsQuery = "SELECT r, w, e, d FROM user_rights WHERE app_user_id = @userId";
            var dataTable = DatabaseManager.Instance.ExecuteQuery(getRightsQuery, new NpgsqlParameter("@userId", userId));

            if (dataTable.Rows.Count > 0)
            {
                // Присвоение прав доступа
                CanRead = Convert.ToBoolean(dataTable.Rows[0]["r"]);
                CanWrite = Convert.ToBoolean(dataTable.Rows[0]["w"]);
                CanEdit = Convert.ToBoolean(dataTable.Rows[0]["e"]);
                CanDelete = Convert.ToBoolean(dataTable.Rows[0]["d"]);
            }
            else
            {
                MessageBox.Show("Права доступа не найдены. Устанавливаются права только для чтения.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                CanRead = true; // Только чтение по умолчанию
                CanWrite = CanEdit = CanDelete = false;
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

        private MenuItem CreateMenuItem(MenuItemData itemData, bool hasChildren)
        {
            var menuItem = new MenuItem
            {
                Header = itemData.Name,
                Tag = itemData.FunctionName
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
                var menuItem = CreateMenuItem(childItem, hasChildren);
                parentMenuItem.Items.Add(menuItem);

                // Рекурсивное добавление дочерних элементов
                AddChildMenuItems(menuItem, menuItems, childItem.Id);
            }
        }


        private void MenuItem_Click(string functionName)
        {
            if (CanRead)
            {
                MenuActionExecutor.Execute(functionName, this);
                ConfigureDataGrid();
            }
        }

        private void ConfigureDataGrid()
        {
            if (MainContent.Children[0] is DataGrid dataGrid)
            {
                // Устанавливаем свойства редактирования и добавления строк
                dataGrid.IsReadOnly = !CanEdit;
                dataGrid.CanUserAddRows = CanWrite;

                // Проверяем, нужно ли добавить колонку для удаления
                if (CanDelete)
                {
                    // Создаем колонку для кнопки удаления
                    var deleteColumn = new DataGridTemplateColumn
                    {
                        Header = "Удалить",
                        CellTemplate = new DataTemplate
                        {
                            VisualTree = CreateDeleteButtonTemplate()
                        }
                    };

                    dataGrid.Columns.Add(deleteColumn);
                }
            }
        }

        private FrameworkElementFactory CreateDeleteButtonTemplate()
        {
            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(Button.ContentProperty, "Удалить");
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteRow_Click));
            return buttonFactory;
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Children[0] is DataGrid dataGrid && dataGrid.SelectedItem is DataRowView selectedRow)
            {
                // Получаем идентификатор строки для удаления
                int id = Convert.ToInt32(selectedRow["id"]);

                // Выполняем удаление строки из базы данных
                string deleteQuery = "DELETE FROM your_table WHERE id = @id";
                DatabaseManager.Instance.ExecuteNonQuery(deleteQuery, new NpgsqlParameter("@id", id));

                // Обновляем данные в таблице
                MenuActionExecutor.Execute("ClassName.MethodName", this);
            }
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
