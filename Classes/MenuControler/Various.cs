using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingCompany.Classes.MenuControler
{
    internal class Various
    {
        public void ChangeAccount(MainWindow mainWindow)
        {
            GlobalRightsDictionary.Clear();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            mainWindow.Close();
        }

        public void ChangePassword(MainWindow mainWindow)
        {
            EditPasswordWindow editPasswordWindow = new EditPasswordWindow(mainWindow.Username);
            editPasswordWindow.ShowDialog();
        }
    }
}
