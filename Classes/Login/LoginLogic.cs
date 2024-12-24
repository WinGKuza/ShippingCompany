using Npgsql;
using ShippingCompany.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShippingCompany.Classes.Login
{
    internal class LoginLogic
    {
        internal static bool TryToLogin(string login, string password)
        {
            string query = "SELECT * FROM app_user WHERE login = @login AND password_hash = @password";
            DataTable dataTable = DatabaseManager.Instance.ExecuteQuery(
                query,
                new NpgsqlParameter("@login", login),
                new NpgsqlParameter("@password", Hash.GetHash(password))
            );

            if (dataTable != null && dataTable.Rows.Count > 0) return true;
            
            return false;
        }
    }
}
