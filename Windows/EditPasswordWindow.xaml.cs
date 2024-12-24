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
            Username = username;
        }

        private void SavePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            if (Hash.GetHash(currentPassword) == newPassword) { }
            if (newPassword == confirmPassword)
            {
                // Логика смены пароля
                MessageBox.Show("Пароль успешно изменен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
