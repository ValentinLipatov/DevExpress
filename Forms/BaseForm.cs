using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Utils;
using System.ComponentModel;

namespace XML
{
    public abstract class BaseForm : XtraForm
    {
        public BaseForm()
        {
            Initialize();

            Load += (s, e) => LoadWorkspace();
            FormClosing += (s, e) => SaveWorkspace();
        }

#if DEBUG
        private string WorkspaceFileName => $"..\\..\\..\\Workspaces\\{Name}.xml";
#else
        private string WorkspaceFileName => $"Workspaces\\{Name}.xml";
#endif

        private List<Field> Fields = new List<Field>();

        private WorkspaceManager WorkspaceManager { get; set; }

        private LayoutControl LayoutControl { get; set; }

        private LayoutControlGroup LayoutControlGroup { get; set; }

        public void LoadWorkspace()
        {
            if (WorkspaceManager.LoadWorkspace(Name, WorkspaceFileName, true))
                WorkspaceManager.ApplyWorkspace(Name);

            CreateGroups();
            RemoveUnusedControls();
            SetCustomizationProperties();
        }

        public void SaveWorkspace()
        {
            WorkspaceManager.CaptureWorkspace(Name, true);
            WorkspaceManager.SaveWorkspace(Name, WorkspaceFileName, true);
        }

        protected T Add<T>(string name, string caption) where T : Control, new()
        {
            return Add<T>(name, caption, caption);
        }

        protected T Add<T>(string name, string caption, string customizationFormCaption) where T : Control, new()
        {
            return Add(new T(), name, caption, customizationFormCaption);
        }

        protected T Add<T>(T control, string name, string caption) where T : Control
        {
            return Add(control, name, caption, caption);
        }

        protected T Add<T>(T control, string name, string caption, string customizationFormCaption) where T : Control
        {
            control.Name = name;

            var layoutControlItem = new LayoutControlItem();
            layoutControlItem.Name = name;
            layoutControlItem.Control = control;
            var customControl = new Field(control, layoutControlItem, caption, customizationFormCaption);

            Fields.Add(customControl);

            return control;
        }

        public void AddTabbedGroup(string name, string caption)
        {
            AddTabbedGroup(name, caption, caption);
        }

        public void AddTabbedGroup(string name, string caption, string customizationFormCaption)
        {
            var tabbedGroup = Contains<TabbedControlGroup>(name);
            if (tabbedGroup == null)
            {
                tabbedGroup = LayoutControl.AddTabbedGroup();
                tabbedGroup.Name = name;
            }
            tabbedGroup.Text = caption;
            tabbedGroup.CustomizationFormText = customizationFormCaption;
        }

        public void AddGroup(string name, string caption)
        {
            AddGroup(name, caption, caption);
        }

        public void AddGroup(string name, string caption, string customizationFormCaption)
        {
            var group = Contains<LayoutControlGroup>(name);
            if (group == null)
            {
                group = LayoutControl.AddGroup();
                group.Name = name;
            }
            group.Text = caption;
            group.CustomizationFormText = customizationFormCaption;
        }

        public void SetCustomizationProperties()
        {
            foreach (var field in Fields)
            {
                if (field.Control is SimpleButton simpleButton)
                    simpleButton.Text = field.Caption;
                else if (field.Control is BaseCheckEdit baseCheckEdit)
                    baseCheckEdit.Text = field.Caption;

                field.LayoutControl.Text = field.Caption;
                field.LayoutControl.CustomizationFormText = field.CustomizationFormCaption;
            }


        }

        protected virtual void CreateFields()
        {
            Add<SkinManagerControl>("SkinManager", "Тема");
            var serializeControlValues = Add<CheckEditControl>("SerializeControlValues", "Сохранять значения полей при закрытии окна");
            serializeControlValues.CheckedChanged += (s, e) => Context.SerializeValues = serializeControlValues.Checked;
            serializeControlValues.AlwaysSerializeValue = true;
        }

        protected virtual void CreateGroups()
        {
            var toRemove = new List<Control>();
            foreach (var item in LayoutControl.Controls)
            {
                if (item is Control control && !Fields.Any(e => e.Control.Name == control.Name))
                    toRemove.Add(control);
            }

            foreach (var item in toRemove)
            {
                LayoutControl.Controls.Remove(item);
            }
        }

        protected T Contains<T>(string name) where T : BaseLayoutItem
        {
            foreach (var item in LayoutControl.Items)
            {
                var result = Contains<T>(name, item);
                if (result != null)
                    return result;
            }

            return null;
        }

        public T Contains<T>(string name, object control) where T : BaseLayoutItem
        {
            if (control is T typedControl && typedControl.Name == name)
            {
                return typedControl;
            }
            else if (control is LayoutControlGroup layoutControlGroup)
            {
                foreach (var item in layoutControlGroup.Items)
                {
                    var result = Contains<T>(name, item);
                    if (result != null)
                        return result;
                }
            }
            else if (control is TabbedControlGroup tabbedControlGroup)
            {
                foreach (var item in tabbedControlGroup.TabPages)
                {
                    var result = Contains<T>(name, item);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        public void RemoveUnusedControls()
        {
            var toRemove = new List<LayoutControlItem>();

            foreach (var item in LayoutControl.Items)
                if (item is LayoutControlItem layoutControlItem && layoutControlItem.IsHidden && layoutControlItem.Control == null)
                    toRemove.Add(layoutControlItem);

            foreach (var item in toRemove)
                LayoutControl.Remove(item, true);
        }

        private void Initialize()
        {
            WorkspaceManager = new WorkspaceManager();
            WorkspaceManager.TargetControl = this;
            WorkspaceManager.SaveTargetControlSettings = true;
            WorkspaceManager.AllowTransitionAnimation = DefaultBoolean.False;

            LayoutControl = new LayoutControl();
            LayoutControlGroup = new LayoutControlGroup();

            CreateFields();

            LayoutControl.SuspendLayout();
            SuspendLayout();

            foreach (var field in Fields)
            {
                if (field.Control is ISupportInitialize supportInitialize)
                    supportInitialize.BeginInit();

                field.LayoutControl.BeginInit();
            }
            LayoutControlGroup.BeginInit();

            LayoutControl.Root = LayoutControlGroup;
            LayoutControl.Dock = DockStyle.Fill;
            LayoutControl.Name = "LayoutControl";
#if RELEASE
            LayoutControl.AllowCustomization = false;
#endif

            foreach (var field in Fields)
            {
                LayoutControl.Controls.Add(field.Control);
                
                if (field.Control is ISupportStyleController controlWithStyleController)
                    controlWithStyleController.StyleController = LayoutControl;
            }

            LayoutControlGroup.Items.AddRange(Fields.Select(e => e.LayoutControl).ToArray());
            LayoutControlGroup.Name = "LayoutControlGroup";
            LayoutControlGroup.TextVisible = false;

            Controls.Add(LayoutControl);
            StartPosition = FormStartPosition.CenterScreen;

            foreach (var field in Fields)
            {
                if (field.Control is ISupportInitialize supportInitialize)
                    supportInitialize.EndInit();

                field.LayoutControl.EndInit();
            }
            LayoutControlGroup.EndInit();

            LayoutControl.ResumeLayout(false);
            ResumeLayout(false);
            
            PerformLayout();
        }
    }
}