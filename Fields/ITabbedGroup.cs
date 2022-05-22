using DevExpress.XtraLayout;

namespace XML
{
    public interface ITabbedGroup
    {
        TabbedControlGroup LayoutControl { get; set; }

        string Caption { get; set; }

        string CustomizationFormCaption { get; set; }  
    }
}