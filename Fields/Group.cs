using DevExpress.XtraLayout;

namespace XML
{
    public class Group : IGroup
    {
        public Group(LayoutControlGroup layoutControl, string caption, string customizationFormCaption)
        {
            LayoutControl = layoutControl;

            Caption = caption;
            CustomizationFormCaption = customizationFormCaption;
        }

        public LayoutControlGroup LayoutControl { get; set; }

        public string Caption { get; set; }

        public string CustomizationFormCaption { get; set; }
    }
}