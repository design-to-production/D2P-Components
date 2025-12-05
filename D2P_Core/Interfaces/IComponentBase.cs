using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IComponentBase : IComponentType, IDocMember
    {
        RhinoDoc ActiveDoc { get; }

        Guid ID { get; set; }
        int GroupIndex { get; }
        string Name { get; }
        string ShortName { get; }
        Plane Plane { get; }

        //IComponentBase Parent { get; }
        //IEnumerable<IComponentBase> Children { get; }
        IEnumerable<IMember> Members { get; }

    }
}
