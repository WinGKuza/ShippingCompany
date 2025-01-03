﻿using ShippingCompany.Database;
using System.Data;
using System.Windows.Controls;

namespace ShippingCompany.Classes.MenuControler
{
    internal class MainTables
    {
        /// <summary>
        /// Загрузка данных из таблицы ship_workers.
        /// </summary>
        public void Worker(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "worker", "Работники");
        }

        /// <summary>
        /// Загрузка данных из таблицы client.
        /// </summary>
        public void Client(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "client", "Клиенты");
        }

        /// <summary>
        /// Загрузка данных из таблицы shipment.
        /// </summary>
        public void Shipment(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "shipment", "Партии");
        }

        /// <summary>
        /// Загрузка данных из таблицы cargo.
        /// </summary>
        public void Cargo(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "cargo", "Грузы");
        }

        /// <summary>
        /// Загрузка данных из таблицы ship.
        /// </summary>
        public void Ship(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "ship", "Судна");
        }

        /// <summary>
        /// Загрузка данных из таблицы seaport.
        /// </summary>
        public void Seaport(MainWindow mainWindow)
        {
            TablesWorker.LoadTableFromDatabase(mainWindow, "seaport", "Порты");
        }
    }
}
