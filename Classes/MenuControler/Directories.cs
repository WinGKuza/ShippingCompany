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
            TablesWorker.LoadTableFromDatabase(mainWindow, "ship_type", "Типы суднен");
        }

        public void Route(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "route", "Маршруты");
        }

        public void Status(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "status", "Статусы");
        }

        public void Bank(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "bank", "Банки");
        }

        public void UnitOfMeasurement(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "unit_of_measurement", "Еденицы измерения");
        }

        public void City(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "city", "Города");
        }

        public void Street(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "street", "Улицы");
        }
    }
}
