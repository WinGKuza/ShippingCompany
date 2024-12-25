using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingCompany.Classes.MenuControler
{
    internal class Help
    {
        public void Content(MainWindow mainWindow)
        {
            HelpExecutor.ShowProjectContents(mainWindow, userId => mainWindow.GetUserMenuItems(userId), "Содержание");
        }

        public void AboutProgram(MainWindow mainWindow)
        {
            HelpExecutor.ShowProjectInfo(mainWindow, "О программе");
        }
    }
}
