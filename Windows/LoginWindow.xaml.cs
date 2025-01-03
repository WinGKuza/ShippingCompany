﻿using Npgsql;
using ShippingCompany.Classes.Login;
using ShippingCompany.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShippingCompany
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri(@"C:\Users\danil\source\repos\DataBase\ShippingCompany\Icons\enter.png"));
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = textbox_login.Text, password = passwordbox_password.Password;
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                if (DatabaseManager.Instance.TestConnection())
                {
                    if (LoginLogic.TryToLogin(login, password))
                    {
                        MainWindow mainWindow = new MainWindow(login);
                        mainWindow.Show();
                        Close();
                    }
                    else
                    {
                        error_message.Text = "Неверный логин или пароль!";
                    }
                }
                else
                {
                    error_message.Text = "Не удалось подключиться к БД!";
                }
            }
            else
            {
                MessageBox.Show("Логин и/или пароль не могут быть пустыми!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
