using D2P_Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_Core.Components {
    public static class ComponentTable {
        static Dictionary<string, Type> Table { get; } = new Dictionary<string, Type>();

        public static IEnumerable<string> Keys => Table.Keys;
        public static IEnumerable<Type> Values => Table.Values;

        public static void RegisterComponent<T>(string typeID) where T : class, IComponentBase
        {
            Table[typeID] = typeof(T);
        }

        public static bool TryGetValue(string typeId, out Type type)
        {
            return Table.TryGetValue(typeId, out type);
        }
        public static bool TryGetTypeId(Type type, out string typeId)
        {
            if (!Table.ContainsValue(type))
                throw new Exception($"No TypeId set for {type.Name}");
            typeId = Table.First(e => e.Value == type).Key;
            return true;
        }
    }

}
