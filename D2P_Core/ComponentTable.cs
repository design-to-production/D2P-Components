using System;
using System.Collections.Generic;

namespace D2P_Core
{
    public static class ComponentTable
    {
        static Dictionary<string, Type> Table { get; } = new Dictionary<string, Type>();

        static ComponentTable()
        {
            RegisterComponent<Component>("");
        }

        public static void RegisterComponent<T>(string typeID) where T : Component, new()
        {
            Table[typeID] = typeof(T);
        }

        public static bool TryGetValue(string typeId, out Type type)
        {
            return Table.TryGetValue(typeId, out type);
        }
    }

}
