using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Utils;
using System.ComponentModel;
using DevExpress.LookAndFeel;
using System.Resources;
using System.Drawing;
using System;

namespace DevExpress
{
    public abstract class BaseForm : XtraForm
    {
        public BaseForm()
        {
            Load += (s, e) => OnLoad();
            FormClosing += (s, e) => OnClose();
        }

#if DEBUG
        protected string WorkspaceFileName => $"..\\..\\..\\Workspaces\\{Name}.xml";
#else
        private string WorkspaceFileName => $"Workspaces\\{Name}.xml";
#endif
        protected List<LayoutControlItem> LayoutControls { get; set; } = new List<LayoutControlItem>();
        protected WorkspaceManager WorkspaceManager { get; set; }
        protected LayoutControl LayoutControl { get; set; }

        protected void LoadWorkspace()
        {
            if (WorkspaceManager.LoadWorkspace(Name, WorkspaceFileName, true))
                WorkspaceManager.ApplyWorkspace(Name);

            CreateLayouts();
            RemoveUnusedLayouts();
        }

        public virtual void OnLoad()
        {
            LoadWorkspace();
        }

        public virtual void OnClose()
        {
            SaveWorkspace();
        }

        protected void SaveWorkspace()
        {
            WorkspaceManager.CaptureWorkspace(Name, true);
            WorkspaceManager.SaveWorkspace(Name, WorkspaceFileName, true);
        }

        public Control AddControl(Type controlType, string name, string caption)
        {
            if (controlType.IsAssignableFrom(typeof(Control)))
                throw new InvalidOperationException();

            var control = (Control)Activator.CreateInstance(controlType);
            return AddControl(control, name, caption);
        }

        public T AddControl<T>(string name, string caption) where T : Control, new()
        {
            return AddControl<T>(new T(), name, caption);
        }

        public T AddControl<T>(T control, string name, string caption) where T : Control
        {
            control.Name = name;
            if (control is SimpleButton simpleButton)
                simpleButton.Text = caption;
            else if (control is BaseCheckEdit baseCheckEdit)
                baseCheckEdit.Text = caption;

            var layoutControlItem = new LayoutControlItem();
            layoutControlItem.Name = name;
            layoutControlItem.Control = control;
            layoutControlItem.Text = caption;
            layoutControlItem.CustomizationFormText = $"{layoutControlItem.Text} ({layoutControlItem.Name})";

            LayoutControls.Add(layoutControlItem);

            return control;
        }

        public void AddTabbedGroup(string name, string caption)
        {
            var tabbedGroup = GetLayout<TabbedControlGroup>(name);
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

        public void AddGroup(string name, string caption)
        {
            var group = GetLayout<LayoutControlGroup>(name);
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

        public void AddLabel(string name, string caption)
        {
            var label = GetLayout<SimpleLabelItem>(name);
            if (label == null)
            {
                label = new SimpleLabelItem();
                LayoutControl.AddItem(label);
                label.Name = name;
#if RELEASE
                label.HideToCustomization();
#endif
            }
            label.Text = caption;
        }

        protected abstract void CreateControls();
       
        protected abstract void CreateLayouts();

        protected T GetLayout<T>(string name) where T : BaseLayoutItem
        {
            foreach (var item in LayoutControl.Items)
            {
                var result = GetLayout<T>(name, item);
                if (result != null)
                    return result;
            }

            return null;
        }

        protected T GetLayout<T>(string name, object control) where T : BaseLayoutItem
        {
            if (control is T typedControl && typedControl.Name == name)
            {
                return typedControl;
            }
            else if (control is LayoutControlGroup layoutControlGroup)
            {
                foreach (var item in layoutControlGroup.Items)
                {
                    var result = GetLayout<T>(name, item);
                    if (result != null)
                        return result;
                }
            }
            else if (control is TabbedControlGroup tabbedControlGroup)
            {
                foreach (var item in tabbedControlGroup.TabPages)
                {
                    var result = GetLayout<T>(name, item);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        protected void RemoveUnusedLayouts()
        {
            var toRemove = new List<LayoutControlItem>();

            foreach (var item in LayoutControl.Items)
                if (item is LayoutControlItem layoutControlItem && layoutControlItem.IsHidden && layoutControlItem.Control == null)
                    toRemove.Add(layoutControlItem);

            foreach (var item in toRemove)
                LayoutControl.Remove(item, true);
        }

        protected virtual void Initialize()
        {
            WorkspaceManager = new WorkspaceManager();
            WorkspaceManager.TargetControl = this;
            WorkspaceManager.SaveTargetControlSettings = true;
            WorkspaceManager.AllowTransitionAnimation = DefaultBoolean.False;
            WorkspaceManager.PropertySerializing += OnPropertySerializing;

            LayoutControl = new LayoutControl();
            var layoutControlGroup = new LayoutControlGroup();

            CreateControls();

            LayoutControl.SuspendLayout();
            SuspendLayout();

            foreach (var layoutControl in LayoutControls)
            {
                if (layoutControl.Control is ISupportInitialize supportInitialize)
                    supportInitialize.BeginInit();

                layoutControl.BeginInit();
            }
            layoutControlGroup.BeginInit();

            LayoutControl.Root = layoutControlGroup;
            LayoutControl.Dock = DockStyle.Fill;
            LayoutControl.Name = "LayoutControl";
#if RELEASE
            LayoutControl.AllowCustomization = false;
#endif

            foreach (var layoutControl in LayoutControls)
            {
                LayoutControl.Controls.Add(layoutControl.Control);

                if (layoutControl.Control is ISupportStyleController controlWithStyleController)
                    controlWithStyleController.StyleController = LayoutControl;
            }

            layoutControlGroup.Items.AddRange(LayoutControls.ToArray());
            layoutControlGroup.Name = "LayoutControlGroup";
            layoutControlGroup.TextVisible = false;

            Controls.Add(LayoutControl);
            StartPosition = FormStartPosition.CenterScreen;

            foreach (var layoutControl in LayoutControls)
            {
                if (layoutControl.Control is ISupportInitialize supportInitialize)
                    supportInitialize.EndInit();
                
                layoutControl.EndInit();
            }
            layoutControlGroup.EndInit();

            LayoutControl.ResumeLayout(false);
            ResumeLayout(false);

            PerformLayout();
        }

        private void OnPropertySerializing(object sender, PropertyCancelEventArgs e)
        {
            if (e.PropertyName == "Text" || e.PropertyName == "CustomizationFormText")
                e.Cancel = true;
        }

        protected void SetIcon(string name)
        {
            var resourceManager = new ResourceManager(GetType());
            IconOptions.Icon = (Icon)resourceManager.GetObject(name);
        }

        protected void SetSkin(string name)
        {
            UserLookAndFeel.Default.SetSkinStyle(name);
        }

        public void SetValue(string name, object value)
        {
            var layoutControl = LayoutControls.FirstOrDefault(e => e.Name == name);
            if (layoutControl == null)
                throw new InvalidOperationException();
            else if (layoutControl.Control is BaseEdit control)
                control.EditValue = value;
        }
    }
}