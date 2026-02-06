using D2P.Core.Interfaces;
using System.Collections.Generic;

namespace D2P.Core.Extensions {
    public static class BaseObjectExtensions {
        public static IEnumerable<IBaseObject> Duplicate(this IEnumerable<IBaseObject> baseObjects)
        {
            foreach (var obj in baseObjects) {
                yield return obj.Duplicate();
            }
        }
    }
}
