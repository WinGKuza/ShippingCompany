using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShippingCompany.Classes.Login
{

    public class User
    {
        public User() { }
        public User(string login, string password)
        {
            Login = login;
            Password = password;
        }
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
