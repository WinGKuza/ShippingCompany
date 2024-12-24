using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShippingCompany.Classes.Login
{
    internal class LoginLogic
    {
        internal static bool TryToLogin(string login, string password, List<User> accounts)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || accounts == null) return false;
            foreach (User user in accounts)
            {
                if (user.Login == login && user.Password == Hash.GetHash(password)) return true;
            }
            //MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}
