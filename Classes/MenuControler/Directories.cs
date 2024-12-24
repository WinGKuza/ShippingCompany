using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingCompany.Classes.MenuControler
{
    internal class Directories
    {
        public void ShipType(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "ship_type");
        }

        public void Route(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "route");
        }

        public void Status(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "status");
        }

        public void Bank(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "bank");
        }

        public void UnitOfMeasurement(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "unit_of_measurement");
        }

        public void City(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "city");
        }

        public void Street(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "street");
        }
    }
}
