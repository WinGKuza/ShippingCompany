using Npgsql;
using ShippingCompany.Classes.Login;
using ShippingCompany.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShippingCompany
{
    /// <summary>
    /// Логика взаимодействия для EditPasswordWindow.xaml
    /// </summary>
    public partial class EditPasswordWindow : Window
    {
        public string Username { get; private set; }

        public EditPasswordWindow(string username)
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri(@"C:\Users\danil\source\repos\DataBase\ShippingCompany\Icons\edit.png"));
            Username = username;
        }

        private void SavePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            if (LoginLogic.TryToLogin(Username, currentPassword))
            {
                if (newPassword == confirmPassword)
                {
                    // Логика смены пароля
                    // Обновляем пароль
                    string updatePasswordQuery = @"
                    UPDATE app_user 
                    SET password_hash = @newPassword
                    WHERE login = @login";
                    int rowsAffected = DatabaseManager.Instance.ExecuteNonQuery(updatePasswordQuery,
                        new NpgsqlParameter("@login", Username),
                        new NpgsqlParameter("@newPassword", Hash.GetHash(newPassword)));

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Пароль успешно изменён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось изменить пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    Close();
                }
                else
                {
                    MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверный пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
