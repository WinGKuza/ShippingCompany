using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingCompany.Classes.MenuControler
{
    internal class Documents
    {
        public void ComandLineSQL(MainWindow mainWindow)
        {
            SqlQueryExecutor.ShowSqlQueryInterface(mainWindow, "Документы");

        }

        public void ShipmentsWithoutShipments(MainWindow mainWindow)
        {
            SqlQueryExecutor.ShowSqlQueryInterface(mainWindow, "Документы", "SELECT c.name\r\nFROM cargo c\r\nLEFT JOIN cargo_in_shipment cis ON c.id = cis.cargo_id\r\nWHERE cis.cargo_id IS NULL;\r\n");
        }

        public void EmployeesWithoutWorkExperience(MainWindow mainWindow)
        {
            SqlQueryExecutor.ShowSqlQueryInterface(mainWindow, "Документы", "SELECT last_name, first_name, middle_name\r\nFROM worker\r\nWHERE work_experience = NULL;\r\n");
        }

        public void DeliveredCargo(MainWindow mainWindow)
        {
            SqlQueryExecutor.ShowSqlQueryInterface(mainWindow, "Документы", "SELECT c.name AS cargo_name, cl.last_name AS client_last_name, cl.first_name AS client_first_name, s.name AS status_name\r\nFROM cargo c\r\nJOIN cargo_in_shipment cis ON c.id = cis.cargo_id\r\nJOIN shipment sh ON cis.shipment_id = sh.id\r\nJOIN client cl ON sh.client_id = cl.id\r\nJOIN status s ON cl.status_id = s.id\r\nWHERE s.name = 'Доставлен';\r\n");
        }
    }
}
