using D2P_Core.Components.Grasshopper;
using D2P_Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_Core.Components
{
    public static class ComponentTable
    {
        static Dictionary<string, Type> Table { get; } = new Dictionary<string, Type>();
        public static string[] TypeIDs => Table.Keys.ToArray();

        static ComponentTable()
        {
            RegisterComponent<GrasshopperComponent>("");
        }

        public static void RegisterComponent<T>(string typeID) where T : IComponentBase, new()
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
            {
                typeId = null;
                return false;
            }
            typeId = Table.First(e => e.Value == type).Key;
            return true;
        }
    }

}
