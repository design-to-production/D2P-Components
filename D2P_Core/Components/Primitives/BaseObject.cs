using D2P_Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace D2P_Core.Components.Primitives {

    public class BaseObject<T> : BaseObject, IBaseObject<T> where T : GeometryBase {
        T IBaseObject<T>.Geometry { get => Geometry as T; set => Geometry = value; }

        public BaseObject(RhinoObject rhObj) : base(rhObj) { }
        public BaseObject(T geometry) : base(geometry) { }
        public BaseObject(T geometry, ObjectAttributes attributes) : base(geometry, attributes) { }

        public new IBaseObject<T> Duplicate()
        {
            return new BaseObject<T>((T)Geometry.Duplicate(), Attributes.Duplicate());
        }
    }

    public class BaseObject : IBaseObject {
        public GeometryBase Geometry { get; set; }
        public ObjectAttributes Attributes { get; set; }

        public BaseObject(RhinoObject rhObj) : this(rhObj.Geometry, rhObj.Attributes) { }
        public BaseObject(GeometryBase geometry) : this(geometry, new ObjectAttributes()) { }
        public BaseObject(GeometryBase geometry, ObjectAttributes attributes)
        {
            Geometry = geometry;
            Attributes = attributes;
        }
        private BaseObject(IBaseObject other)
        {
            Geometry = other.Geometry.Duplicate();
            Attributes = other.Attributes.Duplicate();
        }

        public IBaseObject Duplicate()
        {
            return new BaseObject(this);
        }
    }
}
