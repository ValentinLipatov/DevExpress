using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Mask;

namespace XML
{
    public class XMLForm : BaseForm
    {
        public XMLForm()
        {
            Name = "XMLForm";
            Text = "Сравнение XML";

            var resourceManager = new ResourceManager(GetType());
            IconOptions.Icon = (Icon)resourceManager.GetObject("Icon");
            UserLookAndFeel.Default.SetSkinStyle("Office 2010 Blue");
        }

        protected TextEdit SQLServerName1 { get; set; }
        protected TextEdit SQLServerName2 { get; set; }
        protected TextEdit DatabaseName1 { get; set; }
        protected TextEdit DatabaseName2 { get; set; }
        protected TextEdit TableName1 { get; set; }
        protected TextEdit TableName2 { get; set; }
        protected TextEdit IdColumnName1 { get; set; }
        protected TextEdit IdColumnName2 { get; set; }
        protected TextEdit XMLColumnName1 { get; set; }
        protected TextEdit XMLColumnName2 { get; set; }
        protected TextEdit Id1 { get; set; }
        protected TextEdit Id2 { get; set; }
        protected FolderBrowserControl OutputPath { get; set; }
        protected TextEdit OutputFileName { get; set; }
        protected TextEdit TempFileName1 { get; set; }
        protected TextEdit TempFileName2 { get; set; }
        protected SimpleButton Compare { get; set; }
        protected CheckEdit OpenFile { get; set; }
        protected CheckEdit DeleteTempFiles { get; set; }

        protected override void CreateFields()
        {
            SQLServerName1 = Add<TextEdit>("SQLServerName1", "SQL сервер");
            SQLServerName2 = Add<TextEdit>("SQLServerName2", "SQL сервер");
            DatabaseName1 = Add<TextEdit>("DatabaseName1", "База данных");
            DatabaseName2 = Add<TextEdit>("DatabaseName2", "База данных");
            TableName1 = Add<TextEdit>("TableName1", "Название таблицы");
            TableName2 = Add<TextEdit>("TableName2", "Название таблицы");
            IdColumnName1 = Add<TextEdit>("IdColumnName1", "Название столбца Id");
            IdColumnName2 = Add<TextEdit>("IdColumnName2", "Название столбца Id");
            XMLColumnName1 = Add<TextEdit>("XMLColumnName1", "Название столбца XML");
            XMLColumnName2 = Add<TextEdit>("XMLColumnName2", "Название столбца XML");
            Id1 = Add<TextEdit>("Id1", "Значение Id");
            Id1.Properties.Mask.MaskType = MaskType.Numeric;
            Id1.Properties.Mask.EditMask = @"[0-9]+";
            Id2 = Add<TextEdit>("Id2", "Значение Id");
            Id2.Properties.Mask.MaskType = MaskType.Numeric;
            Id2.Properties.Mask.EditMask = @"^[0-9]+";
            OutputPath = Add<FolderBrowserControl>("OutputPath", "Папка");
            OutputFileName = Add<TextEdit>("OutputFileName", "Название результирующего файла");
            OutputFileName.Properties.Mask.MaskType = MaskType.RegEx;
            OutputFileName.Properties.Mask.EditMask = @".*[.]html";
            TempFileName1 = Add<TextEdit>("TempFileName1", "Название файла c XML A");
            TempFileName1.Properties.Mask.MaskType = MaskType.RegEx;
            TempFileName1.Properties.Mask.EditMask = @".*[.]xml";
            TempFileName2 = Add<TextEdit>("TempFileName2", "Название файла c XML Б");
            TempFileName2.Properties.Mask.MaskType = MaskType.RegEx;
            TempFileName2.Properties.Mask.EditMask = @".*[.]xml";
            DeleteTempFiles = Add<CheckEdit>("DeleteTempFiles", "Удалить файлы с XML после сравнения");
            OpenFile = Add<CheckEdit>("OpenFile", "Открыть результирующий файл после успешного сравнения");
            Compare = Add<SimpleButton>("Compare", "Сравнить");
            Compare.Click += (s, e) => DoCompare();
        }

        protected override void CreateGroups()
        {
            AddGroup("Group1", "Параметры А");
            AddGroup("Group2", "Параметры Б");
            AddGroup("Group3", "Параметры результатов");
        }

        private string CheckMandatoryField(BaseEdit control, ref bool valid)
        {
            var text = control.Text;

            if (string.IsNullOrEmpty(text))
            {
                control.ErrorText = "Не заполнено обязательное поле";
                valid = false;
            }

            return text;
        }

        private void DoCompare()
        {
            try
            {
                bool valid = true;

                string sqlServerName1 = CheckMandatoryField(SQLServerName1, ref valid);
                string sqlServerName2 = CheckMandatoryField(SQLServerName2, ref valid);
                string databaseName1 = CheckMandatoryField(DatabaseName1, ref valid);
                string databaseName2 = CheckMandatoryField(DatabaseName2, ref valid);
                string tableName1 = CheckMandatoryField(TableName1, ref valid);
                string tableName2 = CheckMandatoryField(TableName2, ref valid);
                string idColumnName1 = CheckMandatoryField(IdColumnName1, ref valid);
                string idColumnName2 = CheckMandatoryField(IdColumnName2, ref valid);
                string xmlColumnName1 = CheckMandatoryField(XMLColumnName1, ref valid);
                string xmlColumnName2 = CheckMandatoryField(XMLColumnName2, ref valid);
                string id1 = CheckMandatoryField(Id1, ref valid);
                string id2 = CheckMandatoryField(Id2, ref valid);
                string tempFileName1 = CheckMandatoryField(TempFileName1, ref valid);
                string tempFileName2 = CheckMandatoryField(TempFileName2, ref valid);
                string outputPath = CheckMandatoryField(OutputPath, ref valid);
                string outputFileName = CheckMandatoryField(OutputFileName, ref valid);

                if (!valid)
                    return;

                if (string.IsNullOrEmpty(Path.GetExtension(outputFileName)))
                    outputFileName += ".html";

                string connectionString1 = $"Data Source={sqlServerName1};Initial Catalog={databaseName1};Integrated Security=true";
                string query1 = $"SELECT [{xmlColumnName1}] FROM [{tableName1}] WHERE [{idColumnName1}] = {id1}";
                using SqlConnection connection1 = new SqlConnection(connectionString1);
                SqlCommand command1 = new SqlCommand(query1, connection1);

                string connectionString2 = $"Data Source={sqlServerName2};Initial Catalog={databaseName2};Integrated Security=true";
                string query2 = $"SELECT [{xmlColumnName2}] FROM [{tableName2}] WHERE [{idColumnName2}] = {id2}";
                using SqlConnection connection2 = new SqlConnection(connectionString2);
                SqlCommand command2 = new SqlCommand(query2, connection2);

                connection1.Open();
                SqlDataReader reader1 = command1.ExecuteReader();
                string xml1 = null;
                while (reader1.Read())
                {
                    xml1 = reader1[0].ToString();
                    break;
                }
                reader1.Close();
                connection1.Close();

                if (string.IsNullOrEmpty(xml1))
                {
                    XtraMessageBox.Show(this, "Значение XML А не найдено", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                connection2.Open();
                SqlDataReader reader2 = command2.ExecuteReader();
                string xml2 = null;
                while (reader2.Read())
                {
                    xml2 = reader2[0].ToString();
                    break;
                }
                reader2.Close();
                connection2.Close();

                if (string.IsNullOrEmpty(xml2))
                {
                    XtraMessageBox.Show(this, "Значение XML А не найдено", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (outputPath.Last() != '\\')
                    outputPath += "\\";

                string fileName1 = $"{outputPath}\\{tempFileName1}";
                File.WriteAllText(fileName1, xml1);

                string fileName2 = $"{outputPath}\\{tempFileName2}";
                File.WriteAllText(fileName2, xml2);

                string resultFileName = $"{outputPath}\\{outputFileName}";

                var process = Process.Start($"XmlUtil.exe", $"\"{fileName1}\" \"{fileName2}\" \"{resultFileName}\"");
                process.WaitForExit();

                if (DeleteTempFiles.Checked)
                {
                    File.Delete(fileName1);
                    File.Delete(fileName2);
                }

                if (OpenFile.Checked)
                {
                    var openFileProcess = new Process();
                    openFileProcess.StartInfo = new ProcessStartInfo(resultFileName);
                    openFileProcess.StartInfo.UseShellExecute = true;
                    openFileProcess.Start();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}