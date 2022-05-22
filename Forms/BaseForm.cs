using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Utils;
using System;
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

        private List<IField> _fields;
        private List<IField> Fields
        {
            get => _fields ??= new List<IField>();
            set => _fields = value;
        }

        private List<IGroup> _groups;
        private List<IGroup> Groups
        {
            get => _groups ??= new List<IGroup>();
            set => _groups = value;
        }

        private List<ITabbedGroup> _tabbedGroups;
        private List<ITabbedGroup> TabbedGroups
        {
            get => _tabbedGroups ??= new List<ITabbedGroup>();
            set => _tabbedGroups = value;
        }

        private WorkspaceManager WorkspaceManager { get; set; }

        private LayoutControl LayoutControl { get; set; }

        private LayoutControlGroup LayoutControlGroup { get; set; }

        public void LoadWorkspace()
        {
            if (WorkspaceManager.LoadWorkspace(Name, WorkspaceFileName, true))
                WorkspaceManager.ApplyWorkspace(Name);

            RefreshReferences();
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
            layoutControlItem.Name = $"{layoutControlItem.GetType().Name}{name}";
            layoutControlItem.Control = control;
            var customControl = new Field<T>(control, layoutControlItem, caption, customizationFormCaption);

            Fields.Add(customControl);

            return control;
        }

        public TabbedControlGroup AddTabbedGroup(string name, string caption)
        {
            return AddTabbedGroup(name, caption, caption);
        }

        public TabbedControlGroup AddTabbedGroup(string name, string caption, string customizationFormCaption)
        {
            var tabbedControlGroup = LayoutControlGroup.AddTabbedGroup();
            tabbedControlGroup.Name = $"{tabbedControlGroup.GetType().Name}{name}";
            var tabbedGroup = new TabbedGroup(tabbedControlGroup, caption, customizationFormCaption);
            TabbedGroups.Add(tabbedGroup);

            return tabbedControlGroup;
        }

        public LayoutControlGroup AddGroup(string name, string caption)
        {
            return AddGroup(name, caption, caption);
        }

        public LayoutControlGroup AddGroup(string name, string caption, string customizationFormCaption)
        {
            var layoutControlGroup = LayoutControlGroup.AddGroup();
            layoutControlGroup.Name = $"{layoutControlGroup.GetType().Name}{name}";
            layoutControlGroup.TextLocation = Locations.Top;
            var group = new Group(layoutControlGroup, caption, customizationFormCaption);
            Groups.Add(group);

            return layoutControlGroup;
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

            foreach (var group in Groups)
            {
                group.LayoutControl.Text = group.Caption;
                group.LayoutControl.CustomizationFormText = group.CustomizationFormCaption;
            }

            foreach (var tabbedGroup in TabbedGroups)
            {
                tabbedGroup.LayoutControl.Text = tabbedGroup.Caption;
                tabbedGroup.LayoutControl.CustomizationFormText = tabbedGroup.CustomizationFormCaption;
            }
        }

        protected virtual void Create()
        {
            Add<SkinManagerControl>("SkinManager", "Тема");
            var serializeControlValues = Add<CheckEditControl>("SerializeControlValues", "Сохранять значения полей при закрытии окна");
            serializeControlValues.CheckedChanged += (s, e) => Context.SerializeValues = serializeControlValues.Checked;
            serializeControlValues.AlwaysSerializeValue = true;
        }

        public bool Contains(IGroup group, object item)
        {
            if (item is LayoutControlGroup layoutControlGroup)
            {
                if (layoutControlGroup.Name == group.LayoutControl.Name)
                {
                    group.LayoutControl = layoutControlGroup;
                    return true;
                }
                else
                {
                    foreach (var control in layoutControlGroup.Items)
                    {
                        if (Contains(group, control))
                        {
                            return true;
                        }
                    }
                }
            }
            else if (item is TabbedControlGroup tabbedControlGroup)
            {
                foreach (var page in tabbedControlGroup.TabPages)
                {
                    if (Contains(group, page))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Contains(ITabbedGroup tabbedGroup, object item)
        {
            if (item is LayoutControlGroup layoutControlGroup)
            {
                foreach(var control in layoutControlGroup.Items)
                {
                    if (Contains(tabbedGroup, control))
                    {
                        return true;
                    }
                }
            }
            if (item is TabbedControlGroup tabbedControlGroup)
            {
                if (tabbedControlGroup.Name == tabbedGroup.LayoutControl.Name)
                {
                    tabbedGroup.LayoutControl = tabbedControlGroup;
                    return true;
                }
                foreach (var page in tabbedControlGroup.TabPages)
                {
                    if (Contains(tabbedGroup, page))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected void RefreshReferences()
        {
            foreach (var group in Groups)
            {
                bool contains = false;
                foreach (var item in LayoutControl.Items)
                {
                    if (Contains(group, item))
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    var layoutControlGroup = LayoutControl.AddGroup();
                    layoutControlGroup.Name = group.LayoutControl.Name;
                    group.LayoutControl = layoutControlGroup;
                }
            }

            foreach (var tabbedGroup in TabbedGroups)
            {
                bool contains = false;
                foreach (var item in LayoutControl.Items)
                {
                    if (Contains(tabbedGroup, item))
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    var tabbedControlGroup = LayoutControl.AddTabbedGroup();
                    tabbedControlGroup.Name = tabbedGroup.LayoutControl.Name;
                    tabbedGroup.LayoutControl = tabbedControlGroup;
                }
            }

            var itemsToRemove = new List<BaseLayoutItem>();
            foreach (var item in LayoutControl.Items)
            {
                switch (item)
                {
                    case LayoutControlGroup layoutControlGroup:
                        if (layoutControlGroup != LayoutControl.Root && !Groups.Any(e => e.LayoutControl.Name == layoutControlGroup.Name))
                            itemsToRemove.Add(layoutControlGroup);
                        break;

                    case TabbedControlGroup tabbedControlGroup:
                        if (!TabbedGroups.Any(e => e.LayoutControl.Name == tabbedControlGroup.Name))
                            itemsToRemove.Add(tabbedControlGroup);
                        break;
                }
            }
            foreach (var item in itemsToRemove)
            {
                LayoutControl.Remove(item, true);
            }
        }

        private void Initialize()
        {
            WorkspaceManager = new WorkspaceManager();
            WorkspaceManager.TargetControl = this;
            WorkspaceManager.SaveTargetControlSettings = true;
            WorkspaceManager.AllowTransitionAnimation = DefaultBoolean.False;

            LayoutControl = new LayoutControl();
            LayoutControlGroup = new LayoutControlGroup();

            Create();

            LayoutControl.SuspendLayout();
            foreach (var field in Fields)
               field.Control.SuspendLayout();
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

            foreach (var field in Fields)
                field.Control.ResumeLayout();
            LayoutControl.ResumeLayout(false);
            ResumeLayout(false);
            
            PerformLayout();
        }
    }
}