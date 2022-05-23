using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System.Windows.Forms;

namespace XML
{
    public class FolderBrowserControl : ButtonEdit
    {
        public FolderBrowserControl()
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