using DevExpress.XtraEditors;

namespace XML
{
    public class NumericControl : SpinEdit
    {
        public NumericControl()
        {
            Properties.MaskSettings.MaskExpression = "D";
            Properties.UseMaskAsDisplayFormat = true;
            Properties.MinValue = 0;
        }
    }
}