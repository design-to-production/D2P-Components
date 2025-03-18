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

            Assert.IsTrue(component.ID != Guid.Empty);
        }
    }
}
