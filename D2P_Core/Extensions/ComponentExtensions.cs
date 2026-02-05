using D2P_Core.Interfaces;
using System.Collections.Generic;

namespace D2P_Core.Extensions {
    public static class ComponentExtensions {
        public static void Commit(this IEnumerable<IComponentBase> components)
        {
            foreach (var comp in components) {
                comp.Commit();
            }
        }
    }
}