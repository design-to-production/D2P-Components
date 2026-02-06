using D2P.Core.Interfaces;
using System.Collections.Generic;

namespace D2P.Core.Extensions {
    public static class ComponentExtensions {
        public static void Commit(this IEnumerable<IComponentBase> components)
        {
            foreach (var comp in components) {
                comp.Commit();
            }
        }
    }
}