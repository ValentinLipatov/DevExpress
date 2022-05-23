using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Utils;
using System.ComponentModel;
using System;

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

        private HashSet<string> Names = new HashSet<string>();

        private WorkspaceManager WorkspaceManager { get; set; }

        private LayoutControl LayoutControl { get; set; }

        private LayoutControlGroup LayoutControlGroup { get; set; }

        private void LoadWorkspace()
        {
            if (WorkspaceManager.LoadWorkspace(Name, WorkspaceFileName, true))
                WorkspaceManager.ApplyWorkspace(Name);

            CreateGroups();
            RemoveUnusedControls();
            SetCustomizationProperties();
        }

        private void SaveWorkspace()
        {
            WorkspaceManager.CaptureWorkspace(Name, true);
            WorkspaceManager.SaveWorkspace(Name, WorkspaceFileName, true);
        }

        protected T Add<T>(string name, string caption) where T : Control, new()
        {
            return Add(new T(), name, caption);
        }

        protected T Add<T>(T control, string name, string caption) where T : Control
        {
            CheckName(name);

            control.Name = name;
            var layoutControlItem = new LayoutControlItem();
            layoutControlItem.Name = name;
            layoutControlItem.Control = control;
            var customControl = new Field(layoutControlItem, name, caption);

            Fields.Add(customControl);

            return control;
        }

        protected void AddTabbedGroup(string name, string caption)
        {
            CheckName(name);

            var tabbedGroup = Contains<TabbedControlGroup>(name);
            if (tabbedGroup == null)
            {
                tabbedGroup = LayoutControl.AddTabbedGroup();
                tabbedGroup.Name = name;
#if RELEASE
                tabbedGroup.HideToCustomization();
#endif
            }
            tabbedGroup.Text = caption;
            tabbedGroup.CustomizationFormText = $"{tabbedGroup.Text} ({tabbedGroup.Name})";
        }

        protected void AddGroup(string name, string caption)
        {
            CheckName(name);

            var group = Contains<LayoutControlGroup>(name);
            if (group == null)
            {
                group = LayoutControl.AddGroup();
                group.Name = name;
#if RELEASE
                group.HideToCustomization();
#endif
            }
            group.Text = caption;
            group.CustomizationFormText = $"{group.Text} ({group.Name})";
        }

        private void SetCustomizationProperties()
        {
            foreach (var field in Fields)
            {
                var layoutControlItem = Contains<LayoutControlItem>(field.Name);

                if (layoutControlItem.Control is SimpleButton simpleButton)
                    simpleButton.Text = field.Caption;
                else if (layoutControlItem.Control is BaseCheckEdit baseCheckEdit)
                    baseCheckEdit.Text = field.Caption;

                layoutControlItem.Text = field.Caption;
                layoutControlItem.CustomizationFormText = $"{layoutControlItem.Text} ({layoutControlItem.Name})";
            }
        }

        protected abstract void CreateFields();

        protected abstract void CreateGroups();

        private T Contains<T>(string name) where T : BaseLayoutItem
        {
            foreach (var item in LayoutControl.Items)
            {
                var result = Contains<T>(name, item);
                if (result != null)
                    return result;
            }

            return null;
        }

        private T Contains<T>(string name, object control) where T : BaseLayoutItem
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

        private void RemoveUnusedControls()
        {
            var toRemove = new List<LayoutControlItem>();

            foreach (var item in LayoutControl.Items)
                if (item is LayoutControlItem layoutControlItem && layoutControlItem.IsHidden && layoutControlItem.Control == null)
                    toRemove.Add(layoutControlItem);

            foreach (var item in toRemove)
                LayoutControl.Remove(item, true);
        }

        private void CheckName(string name)
        {
            if (Names.Contains(name))
                throw new InvalidOperationException();
            else
                Names.Add(name);
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
                if (field.LayoutControl.Control is ISupportInitialize supportInitialize)
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
                LayoutControl.Controls.Add(field.LayoutControl.Control);
                
                if (field.LayoutControl.Control is ISupportStyleController controlWithStyleController)
                    controlWithStyleController.StyleController = LayoutControl;
            }

            LayoutControlGroup.Items.AddRange(Fields.Select(e => e.LayoutControl).ToArray());
            LayoutControlGroup.Name = "LayoutControlGroup";
            LayoutControlGroup.TextVisible = false;

            Controls.Add(LayoutControl);
            StartPosition = FormStartPosition.CenterScreen;

            foreach (var field in Fields)
            {
                if (field.LayoutControl.Control is ISupportInitialize supportInitialize)
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