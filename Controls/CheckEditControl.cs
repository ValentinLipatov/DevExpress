using DevExpress.Utils;
using DevExpress.Utils.Serializing;
using DevExpress.XtraEditors;

namespace XML
{
    public class CheckEditControl : CheckEdit, IXtraSerializable
    {
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
                _serializableValue = value ?? false;
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