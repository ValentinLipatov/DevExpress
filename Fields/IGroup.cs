using DevExpress.XtraLayout;

namespace XML
{
    public interface IGroup
    {
        public LayoutControlGroup LayoutControl { get; set; }

        public string Caption { get; set; }

        public string CustomizationFormCaption { get; set; }  
    }
}