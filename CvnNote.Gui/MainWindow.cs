using System;
using Gtk;

namespace CvnNote.Gui
{
	public partial class MainWindow : Gtk.Window
	{
		public MainWindow() : base(Gtk.WindowType.Toplevel)
		{
			Build();

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
