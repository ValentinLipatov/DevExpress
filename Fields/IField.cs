using DevExpress.XtraLayout;
using System.Windows.Forms;

namespace XML
{
    public interface IField
    {
        LayoutControlItem LayoutControl { get; set; }

        Control Control { get; }

        string Caption { get; set; }

        string CustomizationFormCaption { get; set; }  
    }
}