using D2P_Core.Interfaces;
using System.Collections.Generic;

namespace D2P_Core.Extensions {
    public static class BaseObjectExtensions {
        public static IEnumerable<IBaseObject> Duplicate(this IEnumerable<IBaseObject> baseObjects)
        {
            foreach (var obj in baseObjects) {
                yield return obj.Duplicate();
            }
        }
    }
}
