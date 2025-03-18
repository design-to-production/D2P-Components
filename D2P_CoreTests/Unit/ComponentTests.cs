using D2P_Core;
using Rhino.Geometry;

namespace D2P_CoreTests.Unit
{
    [TestClass]
    public sealed class ComponentTests
    {
        [TestMethod]
        public void Component_Constructor()
        {
            var componentType = new ComponentType("TEST", "Testing Type");
            var component = new Component(componentType, "199", Plane.WorldXY);

            Assert.AreNotEqual(Guid.Empty, component.ID);
            Assert.AreEqual(-1, component.GroupIdx);
            Assert.AreEqual("TEST:199", component.Name);
            Assert.AreEqual("199", component.ShortName);
            Assert.IsFalse(component.IsInitialized);
            Assert.IsTrue(component.IsVirtual);
            Assert.IsFalse(component.IsVirtualClone);
            Assert.AreEqual(componentType, component.ComponentType);
            Assert.AreEqual(Plane.WorldXY, component.Plane);
            Assert.IsTrue(component.GeometryCollection.ContainsKey(component.ID));
            Assert.IsTrue(component.AttributeCollection.ContainsKey(component.ID));
        }
    }
}
