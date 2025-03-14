using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IComponent : IComponentType
    {
        RhinoDoc ActiveDoc { get; set; }
        ComponentType ComponentType { get; }

        // IDs
        Guid ID { get; set; }
        int GroupIdx { get; }
        string Name { get; }
        string ShortName { get; }

        // State
        bool IsInitialized { get; }
        bool IsVirtual { get; }
        bool IsVirtualClone { get; }

        // Geometry
        TextEntity Label { get; }
        Plane Plane { get; set; }
        IEnumerable<RhinoObject> RHObjects { get; }
        IEnumerable<GeometryBase> Geometry { get; }
        IEnumerable<ObjectAttributes> Attributes { get; }
        Dictionary<Guid, GeometryBase> GeometryCollection { get; }
        Dictionary<Guid, ObjectAttributes> AttributeCollection { get; }
        Dictionary<ILayerInfo, Dictionary<Guid, int>> StagingLayerCollection { get; }

        // Methods        
        IComponent Clone();
        IComponent VirtualClone();
        bool Transform(Transform xform);
        IList<Guid> AddMember(ComponentMember member);
        IList<Guid> ReplaceMember(ComponentMember member);
        void ClearStagingLayerCollection();
    }
}
