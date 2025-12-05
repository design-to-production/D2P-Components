using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IComponent : IComponentBase, IComponentType
    {
        // Setup
        void Init(RhinoObject obj);

        // IDs
        int GroupIdx { get; }
        string ShortName { get; }

        // State
        bool IsInitialized { get; }
        bool IsVirtual { get; }
        bool IsVirtualClone { get; }

        // Geometry
        TextEntity Label { get; }
        IEnumerable<RhinoObject> RHObjects { get; }
        IEnumerable<GeometryBase> Geometry { get; }
        IEnumerable<ObjectAttributes> Attributes { get; }
        Dictionary<Guid, GeometryBase> GeometryCollection { get; }
        Dictionary<Guid, ObjectAttributes> AttributeCollection { get; }
        Dictionary<ILayerInfo, Dictionary<Guid, int>> StagingLayerCollection { get; }

        // Methods        
        IComponent Clone(bool isVirtual);
        bool Transform(Transform _);
        IList<Guid> AddMember<T>(IMember<T> member) where T : GeometryBase;
        IList<Guid> ReplaceMember<T>(IMember<T> member) where T : GeometryBase;
        void ClearStagingLayerCollection();
    }
}
