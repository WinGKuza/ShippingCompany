using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingCompany.Classes.MenuControler
{
    internal class IntermediateTables
    {
        public void SeaportRoute(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "seaport_route");
        }

        public void ShipRoute(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "ship_route");
        }

        public void ShipWorkers(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "ship_workers");
        }

        public void CargoInShipment(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "cargo_in_shipment");
        }
    }
}
