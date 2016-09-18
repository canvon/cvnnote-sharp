using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Gtk;

namespace CvnNote.Gui
{
	public class NotesElementTreeNode : TreeNode
	{
		public INotesElement NotesElement {
			get;
			private set;
		}


		public NotesElementTreeNode(INotesElement notesElement)
		{
			if (object.ReferenceEquals(notesElement, null))
				throw new ArgumentNullException("notesElement");

			this.NotesElement = notesElement;
		}


		[TreeNodeValue(Column = 0)]
		public string PassiveSummary {
			get {
				return NotesElement.PassiveSummary;
			}
		}

		[TreeNodeValue(Column = 1)]
		public string StartLineNumber {
			get {
				return string.Format("{0}", NotesElement.StartLineNumber);
			}
		}
	}


	public partial class MainWindow : Gtk.Window
	{
		private string _BaseTitle = "cvnnote-sharp GUI";
		public string BaseTitle {
			get {
				return _BaseTitle;
			}
		}

		private string _InfoTitle = null;
		public string InfoTitle {
			get {
				return _InfoTitle;
			}
			set {
				_InfoTitle = value;
				if (_InfoTitle == null)
					this.Title = BaseTitle;
				else
					this.Title = string.Format("{1} - {0}", BaseTitle, _InfoTitle);
			}
		}

		private readonly uint _SbCtxActivity;
		private readonly uint _SbCtxState;
		private readonly uint _SbCtxError;

		private string _FilePath = null;
		public string FilePath {
			get {
				return _FilePath;
			}
			private set {
				_FilePath = value;
				this.entryFilePath.Text = _FilePath ?? String.Empty;
				this.InfoTitle = _FilePath != null ? System.IO.Path.GetFileName(_FilePath) : null;
			}
		}

		private string _RecentApplicationName = "cvnnote-sharp";
		public string RecentApplicationName {
			get {
				return _RecentApplicationName;
			}
			set {
				_RecentApplicationName = value;
				UpdateRecentMenu();
			}
		}

		protected Menu RecentMenu = null;


		public MainWindow() : base(Gtk.WindowType.Toplevel)
		{
			Build();

			// Be sure to start with a consistent window title.
			InfoTitle = null;

			// Prepare using status bar.
			_SbCtxActivity = this.statusbar1.GetContextId("Activity like loading or cleaning up");
			_SbCtxState = this.statusbar1.GetContextId("State of affairs");
			_SbCtxError = this.statusbar1.GetContextId("Error message, should stay visible");

			// Set up NodeView.
			this.nodeviewNotes.NodeStore = new NodeStore(typeof(NotesElementTreeNode));

			this.nodeviewNotes.AppendColumn("Summary", new CellRendererText(), "text", 0);

			var cellRendererLine = new CellRendererText();
			cellRendererLine.Alignment = Pango.Alignment.Right;
			this.nodeviewNotes.AppendColumn("Line", cellRendererLine, "text", 1);

			this.nodeviewNotes.NodeSelection.Changed += NodeviewNotes_NodeSelection_Changed;

			// Set up recent menu.
			Debug.Print("[MainWindow] ctor: Searching for 'Recent' menu...");
			foreach (Widget widget in this.menubar1) {
				Debug.Print("[MainWindow] ctor: Examining widget: {0}", widget);
				var menuItem = widget as MenuItem;
				if (object.ReferenceEquals(menuItem, null)) {
					Debug.Print("[MainWindow] ctor: Widget is not a MenuItem. Skipping.");
					continue;
				}

				var menu = menuItem.Submenu as Menu;
				if (object.ReferenceEquals(menu, null)) {
					Debug.Print("[MainWindow] ctor: MenuItem's submenu is not a Menu. Skipping.");
					continue;
				}

				if (object.ReferenceEquals(menu.Action, this.FileAction)) {
					Debug.Print("[MainWindow] ctor: Menu is not the 'File' menu. Skipping.");
					continue;
				}

				Debug.Print("[MainWindow] ctor: Found 'File' menu.");

				foreach (Widget widget2 in menu) {
					Debug.Print("[MainWindow] ctor: File menu: Examining widget: {0}", widget2);
					var menuItem2 = widget2 as MenuItem;
					if (object.ReferenceEquals(menuItem2, null)) {
						Debug.Print("[MainWindow] ctor: File menu: Widget is not a MenuItem. Skipping.");
						continue;
					}

					if (object.ReferenceEquals(menuItem2.Action, this.RecentAction)) {
						Debug.Print("[MainWindow] ctor: File menu: Found 'Recent' menu item.");
						this.RecentMenu = menuItem2.Submenu as Menu;
						if (object.ReferenceEquals(this.RecentMenu, null)) {
							Debug.Print("[MainWindow] ctor: File menu: Can't handle 'Recent' submenu. Breaking out.");
							break;
						}
						break;
					}
				}

				if (!object.ReferenceEquals(this.RecentMenu, null))
					break;
			}

			if (object.ReferenceEquals(this.RecentMenu, null))
				Debug.Print("[MainWindow] ctor: Couldn't find 'Recent' menu.");

			UpdateRecentMenu();

			// Set up signals. Doing this manually should be cleaner
			// than an association getting lost in the auto-generated code...
			RecentManager.Default.Changed += (sender, e) =>
				UpdateRecentMenu();
			this.openAction.Activated += OpenAction_Activated;
			this.closeAction.Activated += (object sender, EventArgs e) =>
				CloseFile();
			this.quitAction.Activated += (object sender, EventArgs e) =>
				Application.Quit();
		}


		protected void OnDeleteEvent(object sender, DeleteEventArgs a)
		{
			Application.Quit();
			a.RetVal = true;
		}

		public void UpdateRecentMenu()
		{
			// Make sure we know about the recent menu.
			if (object.ReferenceEquals(this.RecentMenu, null)) {
				Debug.Print("[MainWindow] UpdateRecentMenu(): Don't know about a recent menu, skipping.");
				return;
			}

			// Clear recent menu.
			foreach (Widget child in this.RecentMenu.AllChildren) {
				this.RecentMenu.Remove(child);
			}

			// Fill recent menu again.
			// TODO: Try to use RecentChooserMenu instead?
			int recentNumber = 1;
			RecentManager manager = RecentManager.Default;
			foreach (object item in new GLib.List(
				manager.Items.Handle, typeof(RecentInfo), true, true)) {
				// ^ We need to work-around known crash-bug
				//   when using manager.Items directly.

				if (recentNumber > 9)
					// Skip the rest.
					break;

				var info = item as RecentInfo;
				if (object.ReferenceEquals(info, null)) {
					Debug.Print("[MainWindow] UpdateRecentMenu(): RecentManager item is not a RecentInfo: {0}",
						item);
					continue;
				}

				// Only offer files in the recent menu that we registered ourselves.
				if (!info.HasApplication(_RecentApplicationName))
					continue;

				// Actually add a new entry in the recent menu.
				var menuItem = new MenuItem(string.Format("_{0} {1}",
					recentNumber++, info.UriDisplay));
				string uri = info.Uri;
				menuItem.Activated += (object sender, EventArgs e) => LoadFile(uri);
				this.RecentMenu.Add(menuItem);
			}

			Debug.Print("[MainWindow] UpdateRecentMenu(): Created recent menu with {0} entries",
				recentNumber - 1);

			this.RecentMenu.ShowAll();
		}

		void OpenAction_Activated(object sender, EventArgs e)
		{
			// Let the user select a file to open.
			var dialog = new FileChooserDialog(
				string.Format("Open file - {0}", BaseTitle),
				this, FileChooserAction.Open);

			dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
			dialog.AddButton(Stock.Open, ResponseType.Ok);

			int result = dialog.Run();
			string filePath = dialog.Filename;
			dialog.Destroy();

			// Check that the user did in fact give confirmation.
			if (result != (int)ResponseType.Ok)
				return;

			// Actually open the file.
			LoadFile(filePath);
		}

		protected void VisualizeError(string format, params object[] args)
		{
			this.statusbar1.Push(_SbCtxError, string.Format(format, args));

			var dialog = new MessageDialog(
				null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
				format, args);
			dialog.Run();
			dialog.Destroy();
		}

		public void CloseFile()
		{
			this.statusbar1.Pop(_SbCtxState);

			// Clean up.
			try {
				this.statusbar1.Push(_SbCtxActivity, "Cleaning up...");
				Application.RunIteration(false);

				this.textviewText.Buffer.Clear();
				this.nodeviewNotes.NodeStore.Clear();
			}
			finally {
				this.statusbar1.Pop(_SbCtxActivity);
			}

			// Clear error.
			this.statusbar1.Pop(_SbCtxError);

			// Consider file closed.
			this.statusbar1.Push(_SbCtxState, _FilePath != null ? "File closed." : "Cleaned up again.");
			this.FilePath = null;
		}

		public void LoadFile(string filePath)
		{
			// First, ensure a clean state.
			CloseFile();
			this.statusbar1.Pop(_SbCtxState);

			// Indicate what we are there for early.
			//
			// (The idea is that a hypothetically long loading process
			// with the user placing the window in the background
			// would show up in the window list identifiably already.)
			this.FilePath = filePath;

			// Register file as recently used, so that it can be opened easily
			// via the File -> Recent submenu.
			try {
				this.statusbar1.Push(_SbCtxActivity, string.Format(
					"Registering file \"{0}\" as recently used...",
					_FilePath));
				Application.RunIteration(false);

				RecentManager manager = RecentManager.Default;
				var data = new RecentData();
				data.MimeType = "text/plain";
				data.AppName = this.RecentApplicationName;
				// Rather don't record current binary location,
				// it could be a development version...
				// But we have to enter something so that Gtk
				// will let us record the recent info at all,
				// so we make something up here that will be used
				// when the program would be installed system-wide...
				data.AppExec = "cvnnote-gui %u";
				if (manager.AddFull(_FilePath, data) == false) {
					// (I, Fabian, wanted to make this an error
					// that is always somehow reported, but I was told
					// that someone who disables the recent files list
					// would not want log info spam in return ...  So,)
					Debug.Print("[MainWindow] LoadFile(): " +
						"Adding file \"{0}\" to recently used files list did not work.",
						_FilePath);
				}
			}
			finally {
				this.statusbar1.Pop(_SbCtxActivity);
			}

			// Load file from stable storage.
			try {
				this.statusbar1.Push(_SbCtxActivity, string.Format(
					"Loading file \"{0}\"...",
					_FilePath));
				Application.RunIteration(false);

				using (var reader = new StreamReader(_FilePath)) {
					// TODO: Read in text for TextView and for Notes in parallel.
					//       Otherwise, they could diverge...
					// Read in text for TextView.
					TextBuffer buf = this.textviewText.Buffer;
					TextIter end = buf.EndIter;
					buf.Insert(ref end, reader.ReadToEnd());

					// For now, reset the reader to beginning
					// and read everything in again.
					// TODO: (Or can/should we read the data from the TextView?)
					reader.BaseStream.Seek(0, SeekOrigin.Begin);

					// Parse text as Notes.
					var notes = new Notes(reader);

					// Make parse visible to the user.
					AddNotesTree(notes);
				}
			}
			catch (IOException ex) {
				VisualizeError("Error processing file \"{0}\": {1}",
					_FilePath, ex.Message);
				this.FilePath = null;
				return;
			}
			catch (Exception ex) {
				VisualizeError("Unexpected error (file was \"{0}\"): {1}",
					_FilePath, ex.Message);
				this.FilePath = null;
				return;
			}
			finally {
				this.statusbar1.Pop(_SbCtxActivity);
			}

			// Consider file loaded.
			this.statusbar1.Push(_SbCtxState, "File loaded.");
		}

		protected void AddNotesTree(INotesElement tree)
		{
			AddNotesTree(tree, null);
		}

		protected void AddNotesTree(INotesElement tree, NotesElementTreeNode parentNode)
		{
			if (object.ReferenceEquals(tree, null))
				throw new ArgumentNullException("tree");

			var node = new NotesElementTreeNode(tree);

			if (object.ReferenceEquals(parentNode, null))
				this.nodeviewNotes.NodeStore.AddNode(node);
			else
				parentNode.AddChild(node);

			IList<INotesElement> childs = tree.Children;
			if (childs != null) {
				foreach (INotesElement child in childs) {
					AddNotesTree(child, node);
				}
			}
		}

		void NodeviewNotes_NodeSelection_Changed(object sender, EventArgs e)
		{
			// Clear state message at the point of a new user interaction.
			// (If node got *de*-selected, rather have a blank status bar
			// than still the information we would have jumped to line <n>.)
			this.statusbar1.Pop(_SbCtxState);

			// Try to retrieve the currently selected node.
			var node = this.nodeviewNotes.NodeSelection.SelectedNode as NotesElementTreeNode;
			if (object.ReferenceEquals(node, null))
				// Keep state.
				return;

			// Scroll TextView to associated position in the text.
			TextBuffer buf = this.textviewText.Buffer;
			TextIter iter = buf.GetIterAtLineOffset(node.NotesElement.StartLineNumber - 1, 0);
			this.textviewText.ScrollToIter(iter, 0, true, 0, 0.5);

			// Inform the user about what has been done.
			this.statusbar1.Push(_SbCtxState,
				string.Format("Jumped to plaintext line {0}.", node.NotesElement.StartLineNumber));
		}
	}
}
