using D2P_Core.Interfaces;
using D2P_Core.Utility;
using System.Collections.Generic;

namespace D2P_Core.Extensions
{
    public static class ComponentExtensions
    {
        public static void Commit(this IEnumerable<IComponent> components)
        {
            foreach (var comp in components)
            {
                RHDoc.AddToRhinoDoc(comp);
            }
        }
    }
}