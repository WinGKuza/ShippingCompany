using Npgsql;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Username { get; private set; }

        public MainWindow(string username)
        {
            InitializeComponent();
            Username = username;
        }
        private void LoadData()
        {
            // Загрузка данных из базы в DataGridView (пример)
            // DataGridView.ItemsSource = dataSource;
        }

    }
}
