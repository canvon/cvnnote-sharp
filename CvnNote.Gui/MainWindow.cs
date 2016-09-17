using System;
using System.Collections.Generic;
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
				this.InfoTitle = _FilePath;
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

			var cellRendererLine = new CellRendererText();
			cellRendererLine.Alignment = Pango.Alignment.Right;
			this.nodeviewNotes.AppendColumn("Line", cellRendererLine, "text", 1);

			this.nodeviewNotes.NodeSelection.Changed += NodeviewNotes_NodeSelection_Changed;

			// Set up signals. Doing this manually should be cleaner
			// than an association getting lost in the auto-generated code...
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
			// Try to retrieve the currently selected node.
			var node = this.nodeviewNotes.NodeSelection.SelectedNode as NotesElementTreeNode;
			if (object.ReferenceEquals(node, null))
				// Keep state.
				return;

			// Scroll TextView to associated position in the text.
			TextBuffer buf = this.textviewText.Buffer;
			TextIter iter = buf.GetIterAtLineOffset(node.NotesElement.StartLineNumber - 1, 0);
			this.textviewText.ScrollToIter(iter, 0, false, 0, 0);
		}
	}
}
