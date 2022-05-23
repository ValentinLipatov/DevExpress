using DevExpress.XtraLayout;
using System.Windows.Forms;

namespace XML
{
    public class Field
    {
        public Field(LayoutControlItem layoutControl, string name, string caption)
        {
            LayoutControl = layoutControl;
            Name = name;
            Caption = caption;
        }

        public LayoutControlItem LayoutControl { get; set; }

        public string Name { get; set; }

        public string Caption { get; set; }
    }
}