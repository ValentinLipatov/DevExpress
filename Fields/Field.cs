using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Windows.Forms;

namespace XML
{
    public class Field<T> : IField where T : Control
    {
        public Field(T control, LayoutControlItem layoutControl, string caption, string customizationFormCaption)
        {
            Control = control;
            LayoutControl = layoutControl;

            Caption = caption;
            CustomizationFormCaption = customizationFormCaption;
        }

        public LayoutControlItem LayoutControl { get; set; }

        public T Control { get; set; }

        Control IField.Control => Control;

        public string Caption { get; set; }

        public string CustomizationFormCaption { get; set; }
    }
}