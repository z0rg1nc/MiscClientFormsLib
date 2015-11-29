using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace BtmI2p.MiscClientForms
{
	public static class MessageBoxAsync
	{
		public static async Task<DialogResult> ShowAsync(
            Form owner,
			string text,
			string caption = "Error",
			MessageBoxButtons buttons = MessageBoxButtons.OK,
			MessageBoxIcon icon = MessageBoxIcon.Error
        )
		{
		    var fontSize = owner.Font.Size;
            var newForm = new MyMessageBoxForm(
                text,
                caption,
                buttons,
                icon,
                fontSize
            );
            /* Top form close only */
            newForm.Show();
		    return await newForm.ResultTask;
		}
	}

	public static class AsyncExtensions
	{
	    public static async Task InvokeAsync(this Control control, Action action)
	    {
	        if (!control.InvokeRequired)
	        {
	            action();
	        }
	        else
	        {
                var asyncResult = control.BeginInvoke(action);
	            while (!asyncResult.IsCompleted)
	                await Task.Delay(10).ConfigureAwait(false);
                control.EndInvoke(asyncResult);
            }
	    }
        public static async Task InvokeAsync<T1>(
            this T1 control, 
            Action<T1> action
        )
            where T1 : Control
        {
            Assert.NotNull(control);
            Assert.NotNull(action);
            if (!control.InvokeRequired)
            {
                action(control);
            }
            else
            {
                var asyncResult = control.BeginInvoke((Action)(() => action(control)));
                while (!asyncResult.IsCompleted)
                    await Task.Delay(10).ConfigureAwait(false);
                control.EndInvoke(asyncResult);
            }
        }
        public static async Task ShowFormAsync(this Form form, IWin32Window owner)
		{
			var tcs = new TaskCompletionSource<object>();
			form.FormClosed += (sender, args) => tcs.TrySetResult(null);
			form.Show(owner);
			await tcs.Task.ConfigureAwait(false);
		}
        /*
		public static async Task<DialogResult> ShowDialogAsync(this CommonDialog dialog)
		{
			Assert.NotNull(dialog);
			var tcs = new TaskCompletionSource<DialogResult>();
			var thread = new Thread(() =>
			{
				try
				{
					var dialogResult = dialog.ShowDialog();
					tcs.TrySetResult(dialogResult);
				}
				catch (Exception exc)
				{
					tcs.TrySetException(exc);
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			var result = await tcs.Task.ConfigureAwait(false);
			return result;
		}
        */
	}
}
