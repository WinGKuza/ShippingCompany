using System;
using System.Collections.Generic;

namespace ShippingCompany
{
    // Допустим, что для каждого метода мы будем добавлять соответствующие права доступа (r, w, e, d)
    public static class GlobalRightsDictionary
    {
        // Словарь для хранения методов и их прав доступа
        private static readonly Dictionary<string, (bool r, bool w, bool e, bool d)> _dictionary = new Dictionary<string, (bool, bool, bool, bool)>();

        // Метод для добавления или обновления значения
        public static void Set(string key, (bool r, bool w, bool e, bool d) value)
        {
            _dictionary[key] = value;
        }

        // Метод для получения значения
        public static (bool r, bool w, bool e, bool d) Get(string key)
        {
            return _dictionary.TryGetValue(key, out var value) ? value : (false, false, false, false);  // возвращаем дефолтные значения, если ключ не найден
        }

        // Метод для очистки всего словаря
        public static void Clear()
        {
            _dictionary.Clear();
        }

        // Метод для проверки наличия ключа
        public static bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }
    }
}

