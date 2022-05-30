using System;

namespace DevExpress
{
    public class Form : BaseForm
    {
        public Form()
        {
            Name = GetType().Name;

            Initialize();
        }

        protected override void CreateControls()
        {

        }

        protected override void CreateLayouts()
        {

        }
    }
}