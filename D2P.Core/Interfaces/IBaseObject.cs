using Rhino.DocObjects;
using Rhino.Geometry;

namespace D2P.Core.Interfaces {
    public interface IBaseObject {
        GeometryBase Geometry { get; set; }
        ObjectAttributes Attributes { get; set; }
        IBaseObject Duplicate();
    }

    public interface IBaseObject<T> : IBaseObject where T : GeometryBase {
        new T Geometry { get; set; }
        new IBaseObject<T> Duplicate();
    }
}
