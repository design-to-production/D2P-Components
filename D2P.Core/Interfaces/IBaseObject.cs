using Rhino.DocObjects;
using Rhino.Geometry;
using System;

namespace D2P.Core.Interfaces {
    public interface IBaseObject {
        GeometryBase Geometry { get; set; }
        ObjectAttributes Attributes { get; set; }
        Guid Id { get; }
        IBaseObject Duplicate();
    }

    public interface IBaseObject<T> : IBaseObject where T : GeometryBase {
        new T Geometry { get; set; }
        new IBaseObject<T> Duplicate();
    }
}
