using DevExpress.XtraLayout;
using System.Windows.Forms;

namespace XML
{
    public class Field
    {
        public Field(Control control, LayoutControlItem layoutControl, string caption, string customizationFormCaption)
        {
            Control = control;
            LayoutControl = layoutControl;

            Caption = caption;
            CustomizationFormCaption = customizationFormCaption;
        }

        public LayoutControlItem LayoutControl { get; set; }

        public Control Control { get; set; }

        public string Caption { get; set; }

        public string CustomizationFormCaption { get; set; }
    }
}