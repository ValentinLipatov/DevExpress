using DevExpress.Utils;
using DevExpress.Utils.Serializing;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System.Windows.Forms;

namespace XML
{
    public class SaveFileControl : ButtonEdit, IXtraSerializable
    {
        public SaveFileControl()
        {
            ButtonClick += (s, e) =>
            {
                var saveFileDialog = new XtraSaveFileDialog();
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    Text = saveFileDialog.FileName;
            };

            Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }

        public bool SerializeValue { get; set; } = true;

        public bool AlwaysSerializeValue { get; set; } = false;
        
        private object _serializableValue;

        [XtraSerializableProperty]
        public object SerializableValue
        {
            get
            {
                if ((SerializeValue && Context.SerializeValues) || AlwaysSerializeValue)
                    return EditValue;

                return _serializableValue;
            }
            set
            {
                _serializableValue = value;
                EditValue = _serializableValue;
            }
        }

        public void OnEndDeserializing(string restoredVersion)
        {

        }

        public void OnEndSerializing()
        {

        }

        public void OnStartDeserializing(LayoutAllowEventArgs e)
        {

        }

        public void OnStartSerializing()
        {

        }
    }
}