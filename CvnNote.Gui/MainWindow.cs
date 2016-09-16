using System;
using Gtk;

namespace CvnNote.Gui
{
	public partial class MainWindow : Gtk.Window
	{
		public MainWindow() : base(Gtk.WindowType.Toplevel)
		{
			Build();

			this.closeAction.Activated += (object sender, EventArgs e) =>
				// This leaves the application running with no window: this.Destroy();
				// FIXME: Implement closing only the window, but quitting on close of last window!
				Application.Quit();
			this.quitAction.Activated += (object sender, EventArgs e) =>
				Application.Quit();
		}

		protected void OnDeleteEvent(object sender, DeleteEventArgs a)
		{
			Application.Quit();
			a.RetVal = true;
		}
	}
}
