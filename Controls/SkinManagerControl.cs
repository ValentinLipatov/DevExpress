using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Utils;
using DevExpress.Utils.Serializing;
using DevExpress.XtraEditors;
using System.Collections.Generic;

namespace XML
{
    public class SkinManagerControl : LookUpEdit, IXtraSerializable
    {
        public SkinManagerControl()
        {
            var items = new List<string>();
            foreach (SkinContainer skin in SkinManager.Default.Skins)
                items.Add(skin.SkinName);
            
            Properties.DataSource = items;
            EditValueChanged += (s, e) => UserLookAndFeel.Default.SetSkinStyle(EditValue.ToString());
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