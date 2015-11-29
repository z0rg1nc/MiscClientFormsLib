using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using NLog;
using Xunit;

namespace BtmI2p.MiscClientForms
{
    public partial class EditObjectForm : Form
    {
        public static EditObjectFormLocStrings LocStrings 
            = new EditObjectFormLocStrings();
        private EditObjectForm()
        {
            InitializeComponent();
        }

        private EEditObjectFormMode _mode;
        private List<PropertyControlRelation> _controlGenerator;
        public static EditObjectForm CreateInstance<T1>(
            EEditObjectFormMode mode,
            T1 value,
            string caption,
            float fontSize = 9.0f,
            List<string> propNames = null,
            Dictionary<string,string> overridenNames = null 
        )
            where T1 : class
        {
            Assert.NotNull(value);
            Assert.NotNull(caption);
            var controlGenerator
                = PropertyControlRelation.GetSaveControls(
                    value,
                    mode,
                    propNames,
                    overridenNames
                );
            var result = new EditObjectForm();
            result._mode = mode;
            result._controlGenerator = controlGenerator;
            var oldFont = result.Font;
            result.Font = new Font(oldFont.FontFamily,fontSize,oldFont.Style);
            result.Text = caption;
            return result;
        }
        private readonly List<Control> _valueControls = new List<Control>();
        private void AddOrEditForm_Shown(object sender, EventArgs e)
        {
            try
            {
                if (_mode == EEditObjectFormMode.View)
                {
                    addSaveButton.Visible = false;
                }
                else
                {
                    addSaveButton.Text = 
                        _mode == EEditObjectFormMode.Edit 
                            ? LocStrings.Save
                            : LocStrings.Add;
                }
                /**/
                int i = 1;
                SuspendLayout();
                try
                {
                    foreach (var tuple in _controlGenerator)
                    {
                        var rowNum = i++;
                        tableLayoutPanel1.Controls.Add(
                            new Label()
                            {
                                Text = tuple.Name
                            },
                            0,
                            rowNum
                        );
                        var newControl = tuple.CreateControl();
                        _valueControls.Add(newControl);
                        tableLayoutPanel1.Controls.Add(
                            newControl,
                            1,
                            rowNum
                        );
                    }
                }
                finally
                {
                    ResumeLayout();
                }
            }
            catch (Exception exc)
            {
                HandleError(exc);
            }
        }

        public bool ValueChanged = false;
        private void addSaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (
                    Tuple<PropertyControlRelation, Control> tuple 
                    in _controlGenerator.Zip(_valueControls,Tuple.Create)
                )
                {
                    tuple.Item1.SaveValueFromControl(tuple.Item2);
                }
                ValueChanged = true;
                Close();
            }
            catch (Exception exc)
            {
                HandleError(exc);
            }
        }
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private async void HandleError(
            Exception exc,
            [CallerMemberName] string mthdName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _log.Error(
                "{0}:{1} {2}",
                mthdName,
                lineNumber,
                exc.ToString()
            );
            await MessageBoxAsync.ShowAsync(
                this,
                exc.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
