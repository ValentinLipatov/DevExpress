using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Mask;

namespace XML
{
    public class XMLForm : BaseForm
    {
        public XMLForm()
        {
            Name = nameof(XMLForm);
            Text = "Сравнение XML";

            var resourceManager = new ResourceManager(GetType());
            IconOptions.Icon = (Icon)resourceManager.GetObject("Icon");
        }

        protected TextEditControl SQLServerName1 { get; set; }
        protected TextEditControl SQLServerName2 { get; set; }
        protected TextEditControl DatabaseName1 { get; set; }
        protected TextEditControl DatabaseName2 { get; set; }
        protected TextEditControl TableName1 { get; set; }
        protected TextEditControl TableName2 { get; set; }
        protected TextEditControl IdColumnName1 { get; set; }
        protected TextEditControl IdColumnName2 { get; set; }
        protected TextEditControl XMLColumnName1 { get; set; }
        protected TextEditControl XMLColumnName2 { get; set; }
        protected NumericControl Id1 { get; set; }
        protected NumericControl Id2 { get; set; }
        protected FolderBrowserControl OutputPath { get; set; }
        protected TextEditControl OutputFileName { get; set; }
        protected TextEditControl TempFileName1 { get; set; }
        protected TextEditControl TempFileName2 { get; set; }
        protected SimpleButton Compare { get; set; }
        protected CheckEditControl OpenFile { get; set; }
        protected CheckEditControl DeleteTempFiles { get; set; }

        protected override void Create()
        {
            base.Create();

            SQLServerName1 = Add<TextEditControl>(nameof(SQLServerName1), "SQL сервер", "SQL сервер А");
            SQLServerName2 = Add<TextEditControl>(nameof(SQLServerName2), "SQL сервер", "SQL сервер Б");
            DatabaseName1 = Add<TextEditControl>(nameof(DatabaseName1), "База данных", "База данных А");
            DatabaseName2 = Add<TextEditControl>(nameof(DatabaseName2), "База данных", "База данных Б");
            TableName1 = Add<TextEditControl>(nameof(TableName1), "Название таблицы", "Название таблицы А");
            TableName2 = Add<TextEditControl>(nameof(TableName2), "Название таблицы", "Название таблицы Б");
            IdColumnName1 = Add<TextEditControl>(nameof(IdColumnName1), "Название столбца Id", "Название столбца Id А");
            IdColumnName2 = Add<TextEditControl>(nameof(IdColumnName2), "Название столбца Id", "Название столбца Id Б");
            XMLColumnName1 = Add<TextEditControl>(nameof(XMLColumnName1), "Название столбца XML", "Название столбца XML А");
            XMLColumnName2 = Add<TextEditControl>(nameof(XMLColumnName2), "Название столбца XML", "Название столбца XML Б");
            Id1 = Add<NumericControl>(nameof(Id1), "Значение Id", "Значение Id А");
            Id2 = Add<NumericControl>(nameof(Id2), "Значение Id", "Значение Id Б");
            OutputPath = Add<FolderBrowserControl>(nameof(OutputPath), "Папка");
            OutputFileName = Add<TextEditControl>(nameof(OutputFileName), "Название результирующего файла");
            OutputFileName.Properties.Mask.MaskType = MaskType.RegEx;
            OutputFileName.Properties.Mask.EditMask = @".*[.]html";
            TempFileName1 = Add<TextEditControl>(nameof(TempFileName1), "Название файла c XML A");
            TempFileName1.Properties.Mask.MaskType = MaskType.RegEx;
            TempFileName1.Properties.Mask.EditMask = @".*[.]xml";
            TempFileName2 = Add<TextEditControl>(nameof(TempFileName2), "Название файла c XML B");
            TempFileName2.Properties.Mask.MaskType = MaskType.RegEx;
            TempFileName2.Properties.Mask.EditMask = @".*[.]xml";
            DeleteTempFiles = Add<CheckEditControl>(nameof(DeleteTempFiles), "Удалить файлы с XML после сравнения");      
            OpenFile = Add<CheckEditControl>(nameof(OpenFile), "Открыть результирующий файл после успешного сравнения");
            Compare = Add<SimpleButton>(nameof(Compare), "Сравнить", "Сравнить");
            Compare.Click += (s, e) => DoCompare();

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