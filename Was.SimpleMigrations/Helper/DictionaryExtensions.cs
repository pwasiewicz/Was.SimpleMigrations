namespace Was.SimpleMigrations.Helper
{
    using System;
    using System.Collections.Generic;

    internal static class DictionaryExtensions
    {
        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
                                                          TValue defaultValue)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            return !dictionary.ContainsKey(key) ? defaultValue : dictionary[key];
        }
    }
}