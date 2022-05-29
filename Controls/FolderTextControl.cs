using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System.Windows.Forms;

namespace DevExpress
{
    public class FolderTextControl : ButtonEdit
    {
        public FolderTextControl()
        {
            ButtonClick += (s, e) =>
            {
                var folderBrowserDialog = new XtraFolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    Text = folderBrowserDialog.SelectedPath;
            };

            Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }
    }
}