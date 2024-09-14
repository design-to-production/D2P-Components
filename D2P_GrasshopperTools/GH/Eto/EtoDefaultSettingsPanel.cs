using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;

namespace D2P_GrasshopperTools.GH.Eto
{
    internal class EtoDefaultSettingsPanel : Form
    {
        public EtoDefaultSettingsPanel()
        {
            Title = "DefaultSettings";
            var propertyGrid = new TableLayout()
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10, 10, 10, 10),
                DataContext = D2P_GrasshopperTools.Properties.Settings.Default,
                Rows =
                {
                    new EtoTextInputControl("DefaultRootLayerName"),
                    new EtoColorInputControl("DefaultRootLayerColor"),
                    new EtoTextInputControl("DefaultDimensionStyleName"),
                    new EtoCharInputControl("DefaultTypeDelimiter"),
                    new EtoCharInputControl("DefaultLayerDelimiter"),
                    new EtoCharInputControl("DefaultNameDelimiter"),
                    new EtoCharInputControl("DefaultLayerDescriptionDelimiter"),
                    new EtoCharInputControl("DefaultLayerNameDelimiter"),
                    new EtoCharInputControl("DefaultCountDelimiter"),
                    new EtoCharInputControl("DefaultCloneDelimiter"),
                    new EtoCharInputControl("DefaultJointDelimiter"),
                    new TableRow { ScaleHeight = true }
                }
            };
            Content = propertyGrid;
            Focus();
        }
    }

    internal class EtoTextInputControl : TableRow
    {
        public EtoTextInputControl(string propertyName)
            : base(new Label() { Text = propertyName })
        {
            var textBox = new TextBox();
            textBox.TextBinding.BindDataContext<Properties.Settings>(
                p => p[propertyName] as string,
                (p, val) =>
                {
                    p[propertyName] = val;
                    p.Save();
                }
            );
            Cells.Add(textBox);
        }
    }

    internal class EtoColorInputControl : TableRow
    {
        public EtoColorInputControl(string propertyName)
            : base(new Label() { Text = propertyName })
        {
            var colorPicker = new ColorPicker();
            colorPicker.ValueBinding.BindDataContext<Properties.Settings>(
                p => ((System.Drawing.Color)p[propertyName]).ToEto(),
                (p, val) =>
                {
                    p[propertyName] = val.ToSystemDrawing();
                    p.Save();
                }
            );
            Cells.Add(colorPicker);
        }
    }

    internal class EtoCharInputControl : TableRow
    {
        public EtoCharInputControl(string propertyName)
            : base(new Label() { Text = propertyName })
        {
            var textBox = new TextBox() { MaxLength = 1 };
            textBox.TextBinding.BindDataContext<Properties.Settings>(
                p => p[propertyName].ToString(),
                (p, val) =>
                {
                    if (!char.TryParse(val, out char result))
                        return;
                    p[propertyName] = result;
                    p.Save();
                }
            );
            Cells.Add(textBox);
        }
    }
}
