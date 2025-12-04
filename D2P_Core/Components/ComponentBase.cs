using D2P_Core.Interfaces;

namespace D2P_Core.Components
{
    public abstract class ComponentBase<TSelf> : IComponentBase
        where TSelf : ComponentBase<TSelf>
    {
        //public static TSelf Instantiate(RhinoObject rhObj)
        //{
        //    return Instantiation.InstanceFromObject<TSelf>(rhObj);
        //}
    }
}
