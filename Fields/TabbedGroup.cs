using DevExpress.XtraLayout;

namespace XML
{
    public class TabbedGroup : ITabbedGroup
    {
        public TabbedGroup(TabbedControlGroup layoutControl, string caption, string customizationFormCaption)
        {
            LayoutControl = layoutControl;

            Caption = caption;
            CustomizationFormCaption = customizationFormCaption;
        }

        public TabbedControlGroup LayoutControl { get; set; }

        public string Caption { get; set; }

        public string CustomizationFormCaption { get; set; }  
    }
}