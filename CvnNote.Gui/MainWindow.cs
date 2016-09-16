using System;
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
			set {
				if (_FilePath != value)
					CloseFile();

				_FilePath = value;
			}
		}


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

			// Set up signals. Doing this manually should be cleaner
			// than an association getting lost in the auto-generated code...
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

		public void CloseFile()
		{
			// Clean up.
			try {
				this.statusbar1.Push(_SbCtxActivity, "Cleaning up...");
				this.nodeviewNotes.NodeStore.Clear();

				// FIXME
				System.Threading.Thread.Sleep(3000);
			}
			finally {
				this.statusbar1.Pop(_SbCtxActivity);
			}

			// Clear error.
			this.statusbar1.Pop(_SbCtxError);

			// Consider file closed.
			_FilePath = null;
			this.InfoTitle = null;
			this.statusbar1.Push(_SbCtxState, "File closed.");
		}

		public void LoadFile()
		{
			// First, ensure a clean state.
			CloseFile();
			this.statusbar1.Pop(_SbCtxState);

			// Indicate what we are there for early.
			//
			// (The idea is that a hypothetically long loading process
			// with the user placing the window in the background
			// would show up in the window list identifiably already.)
			this.InfoTitle = _FilePath;

			// Load file from stable storage.
			try {
				this.statusbar1.Push(_SbCtxActivity, string.Format(
					"Loading file \"{0}\"...",
					_FilePath));

				using (TextReader reader = new StreamReader(_FilePath)) {
					var notes = new Notes(reader);

					// TODO: Add notes nodes to the store.
				}
			}
			finally {
				this.statusbar1.Pop(_SbCtxActivity);
			}

			// Consider file loaded.
			this.statusbar1.Push(_SbCtxState, "File loaded.");
		}
	}
}
