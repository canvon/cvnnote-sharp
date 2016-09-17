
// This file has been generated by the GUI designer. Do not modify.
namespace CvnNote.Gui
{
	public partial class MainWindow
	{
		private global::Gtk.UIManager UIManager;
		
		private global::Gtk.Action FileAction;
		
		private global::Gtk.Action openAction;
		
		private global::Gtk.Action quitAction;
		
		private global::Gtk.Action closeAction;
		
		private global::Gtk.VBox vbox1;
		
		private global::Gtk.MenuBar menubar1;
		
		private global::Gtk.Toolbar toolbar1;
		
		private global::Gtk.VPaned vpaned1;
		
		private global::Gtk.ScrolledWindow GtkScrolledWindow1;
		
		private global::Gtk.TextView textviewText;
		
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		
		private global::Gtk.NodeView nodeviewNotes;
		
		private global::Gtk.Statusbar statusbar1;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget CvnNote.Gui.MainWindow
			this.UIManager = new global::Gtk.UIManager ();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
			this.FileAction = new global::Gtk.Action ("FileAction", global::Mono.Unix.Catalog.GetString ("_File"), null, null);
			this.FileAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_File");
			w1.Add (this.FileAction, null);
			this.openAction = new global::Gtk.Action ("openAction", global::Mono.Unix.Catalog.GetString ("_Open"), null, "gtk-open");
			this.openAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_Open");
			w1.Add (this.openAction, null);
			this.quitAction = new global::Gtk.Action ("quitAction", global::Mono.Unix.Catalog.GetString ("_Quit"), null, "gtk-quit");
			this.quitAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_Quit");
			w1.Add (this.quitAction, null);
			this.closeAction = new global::Gtk.Action ("closeAction", global::Mono.Unix.Catalog.GetString ("_Close"), null, "gtk-close");
			this.closeAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_Close");
			w1.Add (this.closeAction, null);
			this.UIManager.InsertActionGroup (w1, 0);
			this.AddAccelGroup (this.UIManager.AccelGroup);
			this.Name = "CvnNote.Gui.MainWindow";
			this.Title = global::Mono.Unix.Catalog.GetString ("cvnnote-sharp GUI");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Container child CvnNote.Gui.MainWindow.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString ("<ui><menubar name='menubar1'><menu name='FileAction' action='FileAction'><menuitem name='openAction' action='openAction'/><menuitem name='closeAction' action='closeAction'/><separator/><menuitem name='quitAction' action='quitAction'/></menu></menubar></ui>");
			this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar1")));
			this.menubar1.Name = "menubar1";
			this.vbox1.Add (this.menubar1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.menubar1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString ("<ui><toolbar name='toolbar1'><toolitem name='quitAction' action='quitAction'/><toolitem name='closeAction' action='closeAction'/><toolitem name='openAction' action='openAction'/></toolbar></ui>");
			this.toolbar1 = ((global::Gtk.Toolbar)(this.UIManager.GetWidget ("/toolbar1")));
			this.toolbar1.Name = "toolbar1";
			this.toolbar1.ShowArrow = false;
			this.vbox1.Add (this.toolbar1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.toolbar1]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.vpaned1 = new global::Gtk.VPaned ();
			this.vpaned1.CanFocus = true;
			this.vpaned1.Name = "vpaned1";
			this.vpaned1.Position = 198;
			// Container child vpaned1.Gtk.Paned+PanedChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.textviewText = new global::Gtk.TextView ();
			this.textviewText.CanFocus = true;
			this.textviewText.Name = "textviewText";
			this.textviewText.Editable = false;
			this.textviewText.WrapMode = ((global::Gtk.WrapMode)(3));
			this.GtkScrolledWindow1.Add (this.textviewText);
			this.vpaned1.Add (this.GtkScrolledWindow1);
			global::Gtk.Paned.PanedChild w5 = ((global::Gtk.Paned.PanedChild)(this.vpaned1 [this.GtkScrolledWindow1]));
			w5.Resize = false;
			// Container child vpaned1.Gtk.Paned+PanedChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.nodeviewNotes = new global::Gtk.NodeView ();
			this.nodeviewNotes.CanFocus = true;
			this.nodeviewNotes.Name = "nodeviewNotes";
			this.GtkScrolledWindow.Add (this.nodeviewNotes);
			this.vpaned1.Add (this.GtkScrolledWindow);
			this.vbox1.Add (this.vpaned1);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.vpaned1]));
			w8.Position = 2;
			// Container child vbox1.Gtk.Box+BoxChild
			this.statusbar1 = new global::Gtk.Statusbar ();
			this.statusbar1.Name = "statusbar1";
			this.statusbar1.Spacing = 6;
			this.vbox1.Add (this.statusbar1);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.statusbar1]));
			w9.PackType = ((global::Gtk.PackType)(1));
			w9.Position = 3;
			w9.Expand = false;
			w9.Fill = false;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 690;
			this.DefaultHeight = 503;
			this.Show ();
			this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		}
	}
}
