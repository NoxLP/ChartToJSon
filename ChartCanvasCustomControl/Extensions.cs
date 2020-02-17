using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace
{
    public static class DictionaryExtensions
    {
        public static void AddListOrItemToDictionary<TKey, RListType>(this Dictionary<TKey, List<RListType>> source, TKey key, RListType value, bool duplicateValues = false)
        {
            if (!source.ContainsKey(key))
            {
                source.Add(key, new List<RListType>() { value });
                var forget = Task.Run(() => WPFHelpers.Log.Instance.WriteAsync($"Añadido a diccionario key: {key.ToString()} ; value: {value.ToString()}"));
            }
            else
            {
                if (!duplicateValues && source[key].Contains(value))
                    return;

                var forget = Task.Run(() => WPFHelpers.Log.Instance.WriteAsync($"Añadido a diccionario key: {key.ToString()} ; value: {value.ToString()}"));
                source[key].Add(value);
            }
        }
        public static void RemoveListOrItemFromDictionary<TKey, RListType>(this Dictionary<TKey, List<RListType>> source, TKey key, RListType value)
        {
            if (!source.ContainsKey(key))
                return;

            if (!source[key].Contains(value))
                return;

            var forget = Task.Run(() => WPFHelpers.Log.Instance.WriteAsync($"Borrando de diccionario key: {key.ToString()} ; value: {value.ToString()}"));

            if (source[key].Count < 2)
                source.Remove(key);
            else
                source[key].Remove(value);
        }
    }
}
