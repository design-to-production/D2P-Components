using Rhino.DocObjects;

namespace D2P.Core.Utility
{
    internal static class Constants
    {
        internal static ObjectEnumeratorSettings ObjectEnumeratorSettings(string filter)
        {
            return new ObjectEnumeratorSettings()
            {
                HiddenObjects = true,
                LockedObjects = true,
                NameFilter = filter,
                ObjectTypeFilter = ObjectType.Annotation
            };
        }
    }
}
