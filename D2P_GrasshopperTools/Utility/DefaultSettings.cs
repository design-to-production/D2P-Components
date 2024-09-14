namespace D2P_GrasshopperTools.Utility
{
    internal static class DefaultSettings
    {
        internal static D2P_Core.Settings Create()
        {
            return new D2P_Core.Settings()
            {
                RootLayerName = Properties.Settings.Default.DefaultRootLayerName,
                RootLayerColor = Properties.Settings.Default.DefaultRootLayerColor,
                DimensionStyleName = Properties.Settings.Default.DefaultDimensionStyleName,
                TypeDelimiter = Properties.Settings.Default.DefaultTypeDelimiter,
                LayerDelimiter = Properties.Settings.Default.DefaultLayerDelimiter,
                NameDelimiter = Properties.Settings.Default.DefaultNameDelimiter,
                LayerDescriptionDelimiter = Properties.Settings.Default.DefaultLayerDescriptionDelimiter,
                LayerNameDelimiter = Properties.Settings.Default.DefaultLayerNameDelimiter,
                CountDelimiter = Properties.Settings.Default.DefaultCountDelimiter,
                JointDelimiter = Properties.Settings.Default.DefaultJointDelimiter,
            };
        }
    }
}
