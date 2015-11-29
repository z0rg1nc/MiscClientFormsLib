using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace BtmI2p.MiscClientForms
{
    public partial class MyMessageBoxForm : Form
    {
        private readonly TaskCompletionSource<DialogResult> _dialogResultTcs
            = new TaskCompletionSource<DialogResult>();

        public Task<DialogResult> ResultTask => _dialogResultTcs.Task;
        private readonly string _text;
        private readonly string _caption;
        private readonly MessageBoxButtons _buttons;
        private readonly MessageBoxIcon _icon;
        private readonly float _fontSize;
        public MyMessageBoxForm(
            string text,
            string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Error,
            float fontSize = 9.0f
        )
        {
            Assert.NotNull(text);
            Assert.NotNull(caption);
            _text = text;
            _caption = caption;
            _buttons = buttons;
            _icon = icon;
            _fontSize = fontSize;
            InitializeComponent();
        }

        private void MyMessageBoxForm_Shown(object sender, EventArgs e)
        {
            this.SuspendLayout();
            try
            {
                InitCommonView();
                /**/
                var currentFont = Font;
                Font = new Font(currentFont.FontFamily, _fontSize, currentFont.Style);
                Text = _caption;
                messageTextBox.Text = _text;
                if (_icon == MessageBoxIcon.Error)
                    this.BackColor = Color.Pink;
                else if (_icon == MessageBoxIcon.Question)
                    this.BackColor = Color.Yellow;
                else if (_icon == MessageBoxIcon.Information)
                    this.BackColor = Color.LightSkyBlue;
                foreach (
                    var button in new[]
                    {
                        okButton,
                        abortButton,
                        cancelButton,
                        ignoreButton,
                        noButton,
                        yesButton,
                        retryButton
                    }
                )
                {
                    button.Visible = false;
                }
                switch (_buttons)
                {
                    case MessageBoxButtons.OK:
                        okButton.Visible = true;
                        okButton.Focus();
                        break;
                    case MessageBoxButtons.OKCancel:
                        okButton.Visible = true;
                        cancelButton.Visible = true;
                        okButton.Focus();
                        break;
                    case MessageBoxButtons.AbortRetryIgnore:
                        abortButton.Visible = true;
                        retryButton.Visible = true;
                        ignoreButton.Visible = true;
                        ignoreButton.Focus();
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        yesButton.Visible = true;
                        noButton.Visible = true;
                        cancelButton.Visible = true;
                        yesButton.Focus();
                        break;
                    case MessageBoxButtons.YesNo:
                        yesButton.Visible = true;
                        noButton.Visible = true;
                        yesButton.Focus();
                        break;
                    case MessageBoxButtons.RetryCancel:
                        retryButton.Visible = true;
                        cancelButton.Visible = true;
                        retryButton.Focus();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally
            {
                this.ResumeLayout();
            }
        }

        private void MyMessageBoxForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _dialogResultTcs.TrySetResult(
                DialogResult.None);
        }

        private void yesButton_Click(object sender, EventArgs e)
        {
            _dialogResultTcs.TrySetResult(DialogResult.Yes);
            Close();
        }

        private void noButton_Click(object sender, EventArgs e)
        {
            _dialogResultTcs.TrySetResult(DialogResult.No);
            Close();
        }

        private void ignoreButton_Click(object sender, EventArgs e)
        {
            _dialogResultTcs.TrySetResult(DialogResult.Ignore);
            Close();
        }

        private void abortButton_Click(object sender, EventArgs e)
        {
            _dialogResultTcs.TrySetResult(DialogResult.Abort);
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            _dialogResultTcs.TrySetResult(DialogResult.Cancel);
            Close();
        }

        private void retryButton_Click(object sender, EventArgs e)
        {
            _dialogResultTcs.TrySetResult(DialogResult.Retry);
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _dialogResultTcs.TrySetResult(DialogResult.OK);
            Close();
        }
        public static MyMessageBoxFormDesignerLocStrings DesignerLocStrings = new MyMessageBoxFormDesignerLocStrings();
        private void InitCommonView()
        {
            this.okButton.Text = DesignerLocStrings.OkButtonText;
            this.cancelButton.Text = DesignerLocStrings.CancelButtonText;
            this.retryButton.Text = DesignerLocStrings.RetryButtonText;
            this.abortButton.Text = DesignerLocStrings.AbortButtonText;
            this.ignoreButton.Text = DesignerLocStrings.IgnoreButtonText;
            this.noButton.Text = DesignerLocStrings.NoButtonText;
            this.yesButton.Text = DesignerLocStrings.YesButtonText;
            this.Text = DesignerLocStrings.Text;
        }
    }
    public class MyMessageBoxFormDesignerLocStrings
    {
        public string OkButtonText = "OK";
        public string CancelButtonText = "Cancel";
        public string RetryButtonText = "Retry";
        public string AbortButtonText = "Abort";
        public string IgnoreButtonText = "Ignore";
        public string NoButtonText = "No";
        public string YesButtonText = "Yes";
        public string Text = "Caption";
    }
}
