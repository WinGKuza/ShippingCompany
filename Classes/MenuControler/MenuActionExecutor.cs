using ShippingCompany;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

public static class MenuActionExecutor
{
    public static void Execute(string functionName, params object[] parameters)
    {
        if (string.IsNullOrEmpty(functionName))
        {
            MessageBox.Show("Пустая функция!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Разделяем строку на имя класса и метод
        var parts = functionName.Split('.');
        if (parts.Length == 0)
        {
            MessageBox.Show("Неправильное имя функции!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Имя класса
        string className = parts[0];
        string methodName = parts.Length > 1 ? parts[1] : null;

        try
        {
            // Получаем тип класса
            Type type = Type.GetType($"ShippingCompany.Classes.MenuControler.{className}");
            if (type == null)
            {
                Console.WriteLine();
                MessageBox.Show($"Класс '{className}' не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создаём экземпляр класса
            object instance = Activator.CreateInstance(type);

            if (methodName == null)
            {
                // Если метод не указан, вызывается только конструктор
                Console.WriteLine($"Конструктор классы '{className}' вызван.");
            }
            else
            {
                // Ищем метод
                MethodInfo method = type.GetMethod(methodName);
                if (method == null)
                {
                    MessageBox.Show($"Метод '{methodName}' не найден в классе '{className}'.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Вызываем метод
                method.Invoke(instance, parameters);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при вызове функции '{functionName}': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
