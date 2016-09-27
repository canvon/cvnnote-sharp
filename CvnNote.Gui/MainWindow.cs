/*
 *  cvnnote-sharp - process canvon notes files with C#/.NET library, CLI or GUI
 *  Copyright (C) 2016  Fabian Pietsch <fabian@canvon.de>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Gtk;

namespace CvnNote.Gui
{
	public enum NotesElementTreeNodeType {
		NotesElement,
		ParseIssue,
	}

	public class NotesElementTreeNode : TreeNode
	{
		public NotesElementTreeNodeType NodeType {
			get;
			private set;
		}

		public INotesElement NotesElement {
			get;
			private set;
		}

		public ParseIssue ParseIssue {
			get;
			private set;
		}


		public NotesElementTreeNode(INotesElement notesElement)
		{
			if (object.ReferenceEquals(notesElement, null))
				throw new ArgumentNullException("notesElement");

			this.NodeType = NotesElementTreeNodeType.NotesElement;
			this.NotesElement = notesElement;
			this.ParseIssue = null;
		}

		public NotesElementTreeNode(ParseIssue parseIssue)
		{
			if (object.ReferenceEquals(parseIssue, null))
				throw new ArgumentNullException("parseIssue");

			this.NodeType = NotesElementTreeNodeType.ParseIssue;
			this.NotesElement = null;
			this.ParseIssue = parseIssue;
		}


		[TreeNodeValue(Column = 0)]
		public string PassiveSummary {
			get {
				switch (this.NodeType) {
				case NotesElementTreeNodeType.NotesElement:
					return this.NotesElement.PassiveSummary;
				case NotesElementTreeNodeType.ParseIssue:
					// TODO: Include location information?
					return string.Format("{0}: {1}", this.ParseIssue.Severity, this.ParseIssue.Message);
				default:
					throw new InvalidOperationException(
						string.Format("Invalid node type {0}", this.NodeType));
				}
			}
		}

		[TreeNodeValue(Column = 1)]
		public string StartLineNumber {
			get {
				switch (this.NodeType) {
				case NotesElementTreeNodeType.NotesElement:
					return string.Format("{0}", this.NotesElement.StartLineNumber);
				case NotesElementTreeNodeType.ParseIssue:
					if (this.ParseIssue.Location.StartLine == 0)
						return String.Empty;
					return string.Format(
						"{0}-{1}",
						this.ParseIssue.Location.StartLine,
						this.ParseIssue.Location.EndLine);
				default:
					throw new InvalidOperationException(
						string.Format("Invalid node type {0}", this.NodeType));
				}
			}
		}

		[TreeNodeValue(Column = 2)]
		public string TotalLineCount {
			get {
				switch (this.NodeType) {
				case NotesElementTreeNodeType.NotesElement:
					return string.Format("{0}", NotesElement.TotalLineCount);
				case NotesElementTreeNodeType.ParseIssue:
					int? lineCount = this.ParseIssue.Location.LineCount;
					if (lineCount.HasValue)
						return string.Format("{0}", lineCount.Value);
					else
						return String.Empty;  // "Not applicable"
				default:
					throw new InvalidOperationException(
						string.Format("Invalid node type {0}", this.NodeType));
				}
			}
		}

		[TreeNodeValue(Column = 3)]
		public string Color {
			get {
				switch (this.NodeType) {
				case NotesElementTreeNodeType.NotesElement:
					if (!object.ReferenceEquals(this.NotesElement.ParseIssues, null) &&
					    this.NotesElement.ParseIssues.Count > 0)
						return "red";
					return "black";
				case NotesElementTreeNodeType.ParseIssue:
					return "red";
				default:
					throw new InvalidOperationException(
						string.Format("Invalid node type {0}", this.NodeType));
				}
			}
		}

		[TreeNodeValue(Column = 4)]
		public string NumIssues {
			get {
				switch (this.NodeType) {
				case NotesElementTreeNodeType.NotesElement:
					if (this.NotesElement.TotalParseIssueCount == 0)
						return String.Empty;
					return string.Format("{0}", this.NotesElement.TotalParseIssueCount);
				default:
					return "<-";
				}
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

		protected TextTag
			_TagCurrentNotesElement,
			_TagCurrentNotesElementLinewise,
			_TagErrorParseIssue,
			_TagErrorParseIssueLinewise,
			_TagWarningParseIssue,
			_TagWarningParseIssueLinewise;
		protected Dictionary<SemanticType, TextTag> _TagsSemanticType;

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

		// (See LoadFile() for comments on this.)
		private string _RecentApplicationExec = "cvnnote-gui %u";
		public string RecentApplicationExec {
			get {
				return _RecentApplicationExec;
			}
			set {
				_RecentApplicationExec = value;
				// Probably nothing needs updating, the new setting
				// will be used on next file open action.
			}
		}

		protected Menu RecentMenu = null;

		protected SearchDialog NodeSearchDialog = null;


		public MainWindow() : base(Gtk.WindowType.Toplevel)
		{
			Build();

			// Be sure to start with a consistent window title.
			InfoTitle = null;

			// Prepare using status bar.
			_SbCtxActivity = this.statusbar1.GetContextId("Activity like loading or cleaning up");
			_SbCtxState = this.statusbar1.GetContextId("State of affairs");
			_SbCtxError = this.statusbar1.GetContextId("Error message, should stay visible");

			// Prepare using TextView.
			TextTagTable tagTable = this.textviewText.Buffer.TagTable;

			// Current notes element
			_TagCurrentNotesElement = new TextTag("current_notes_element");
			_TagCurrentNotesElement.Background = "lightgreen";
			_TagCurrentNotesElement.BackgroundSet = true;
			tagTable.Add(_TagCurrentNotesElement);

			_TagCurrentNotesElementLinewise = new TextTag("current_notes_element_linewise");
			_TagCurrentNotesElementLinewise.ParagraphBackground = "lightgreen";
			tagTable.Add(_TagCurrentNotesElementLinewise);

			// Error parse issue
			_TagErrorParseIssue = new TextTag("error_parse_issue");
			_TagErrorParseIssue.Background = "red";
			_TagErrorParseIssue.BackgroundSet = true;
			tagTable.Add(_TagErrorParseIssue);

			_TagErrorParseIssueLinewise = new TextTag("error_parse_issue_linewise");
			_TagErrorParseIssueLinewise.ParagraphBackground = "red";
			tagTable.Add(_TagErrorParseIssueLinewise);

			// Warning parse issue
			_TagWarningParseIssue = new TextTag("warning_parse_issue");
			_TagWarningParseIssue.Background = "orange";
			_TagWarningParseIssue.BackgroundSet = true;
			tagTable.Add(_TagWarningParseIssue);

			_TagWarningParseIssueLinewise = new TextTag("warning_parse_issue_linewise");
			_TagWarningParseIssueLinewise.ParagraphBackground = "orange";
			tagTable.Add(_TagWarningParseIssueLinewise);

			// Semantic types:
			_TagsSemanticType = new Dictionary<SemanticType, TextTag>();

			// Type
			var typeSemanticTypeTag = new TextTag("syntax_type");
			typeSemanticTypeTag.Foreground = "green";
			typeSemanticTypeTag.ForegroundSet = true;
			tagTable.Add(typeSemanticTypeTag);
			this._TagsSemanticType.Add(SemanticType.Type, typeSemanticTypeTag);

			// Identifier
			var identifierSemanticTypeTag = new TextTag("syntax_identifier");
			identifierSemanticTypeTag.Foreground = "darkcyan";
			identifierSemanticTypeTag.ForegroundSet = true;
			tagTable.Add(identifierSemanticTypeTag);
			this._TagsSemanticType.Add(SemanticType.Identifier, identifierSemanticTypeTag);

			// Constant
			var constantSemanticTypeTag = new TextTag("syntax_constant");
			constantSemanticTypeTag.Foreground = "darkred";
			constantSemanticTypeTag.ForegroundSet = true;
			tagTable.Add(constantSemanticTypeTag);
			this._TagsSemanticType.Add(SemanticType.Constant, constantSemanticTypeTag);

			// Keyword
			var keywordSemanticTypeTag = new TextTag("syntax_keyword");
			keywordSemanticTypeTag.Foreground = "darkorange";
			keywordSemanticTypeTag.ForegroundSet = true;
			tagTable.Add(keywordSemanticTypeTag);
			this._TagsSemanticType.Add(SemanticType.Keyword, keywordSemanticTypeTag);


			// Set up NodeView.
			this.nodeviewNotes.NodeStore = new NodeStore(typeof(NotesElementTreeNode));

			this.nodeviewNotes.AppendColumn("Summary", new CellRendererText(), "text", 0, "foreground", 3);

			var cellRendererLine = new CellRendererText();
			cellRendererLine.Alignment = Pango.Alignment.Right;
			this.nodeviewNotes.AppendColumn("Line", cellRendererLine, "text", 1, "foreground", 3);

			var cellRendererTotalLines = new CellRendererText();
			cellRendererTotalLines.Alignment = Pango.Alignment.Right;
			this.nodeviewNotes.AppendColumn("# Lines", cellRendererTotalLines, "text", 2, "foreground", 3);

			var cellRendererParseIssues = new CellRendererText();
			cellRendererParseIssues.Alignment = Pango.Alignment.Right;
			cellRendererParseIssues.Foreground = "red";
			this.nodeviewNotes.AppendColumn("# Issues", cellRendererParseIssues, "text", 4);

			this.nodeviewNotes.NodeSelection.Changed += NodeviewNotes_NodeSelection_Changed;
			this.nodeviewNotes.KeyReleaseEvent += NodeviewNotes_KeyReleaseEvent;

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

		protected override bool OnConfigureEvent(Gdk.EventConfigure evnt)
		{
			base.OnConfigureEvent(evnt);
			PositionSearchDialog();
			return true;
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
				manager.Items.Handle, typeof(RecentInfo), false, false)) {
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

			// Try to start file selection from current file,
			// if there is one.
			//
			// This enables the user to successively open files
			// from the same directory, e.g., from a collection
			// of notes files.
			if (!string.IsNullOrEmpty(_FilePath))
				dialog.SetFilename(_FilePath);

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
				data.AppExec = this.RecentApplicationExec;
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

		/// <summary>
		/// Add a <see cref="Notes"/> tree recursively to the GUI.
		/// See the other overload for details.
		/// </summary>
		/// <param name="tree">The root of the Notes tree.</param>
		protected void AddNotesTree(INotesElement tree)
		{
			AddNotesTree(tree, null);
		}

		/// <summary>
		/// Add a <see cref="Notes"/> tree recursively to the GUI.
		/// This builds up the NodeView NodeStore,
		/// adds formatting to the TextView and
		/// possibly does other things, too.
		/// </summary>
		/// <param name="tree">The current element of the Notes tree.</param>
		/// <param name="parentNode">Optionally, a NodeView parent node
		/// to which to attach a to-be-created node, or <c>null</c>.</param>
		protected void AddNotesTree(INotesElement tree, NotesElementTreeNode parentNode)
		{
			if (object.ReferenceEquals(tree, null))
				throw new ArgumentNullException("tree");

			IList<ISemanticLocatable> syntaxElements = tree.SyntaxElements;
			if (syntaxElements != null) {
				foreach (ISemanticLocatable elem in syntaxElements) {
					ColorizeSyntaxElement(elem);
				}
			}

			var node = new NotesElementTreeNode(tree);

			// Add node as child of the given parent, or as root note if none given.
			if (object.ReferenceEquals(parentNode, null))
				this.nodeviewNotes.NodeStore.AddNode(node);
			else
				parentNode.AddChild(node);

			// Add potential parsing issues as children to the node.
			IList<ParseIssue> issues = tree.ParseIssues;
			if (issues != null) {
				foreach (ParseIssue issue in issues) {
					// Flat (no sub-issues), we can do this without recursion.
					node.AddChild(new NotesElementTreeNode(issue));
					ColorizeParseIssue(issue);
				}
			}

			// Add notes element children as children to the node.
			IList<INotesElement> childs = tree.Children;
			if (childs != null) {
				foreach (INotesElement child in childs) {
					// Do this recursively, as the notes element child
					// can contain further childs as well.
					AddNotesTree(child, node);
				}
			}
		}

		/// <summary>
		/// Colorize TextView buffer range corresponding to
		/// a <see cref="ISemanticLocatable"/> syntax element.
		/// </summary>
		/// <param name="elem">The syntax element.</param>
		protected void ColorizeSyntaxElement(ISemanticLocatable elem)
		{
			if (object.ReferenceEquals(elem, null))
				throw new ArgumentNullException("elem");

			Location loc = elem.Location;
			if (object.ReferenceEquals(loc, null) || loc.StartLine < 1)
				// No location information known; ignore.
				return;

			TextTag tag;
			if (!this._TagsSemanticType.TryGetValue(elem.SemanticType, out tag))
				// Syntax highlighting for this semantic type not known; ignore.
				return;

			TextBuffer buf = this.textviewText.Buffer;
			TextIter start, end;
			if (loc.StartCharacter < 1 && loc.EndCharacter < 1) {
				// Line-wise
				start = buf.GetIterAtLineOffset(loc.StartLine - 1, 0);
				end = buf.GetIterAtLineOffset(loc.EndLine - 1 + 1, 0);
			}
			else {
				// Character-wise
				start = buf.GetIterAtLineOffset(loc.StartLine - 1, loc.StartCharacter - 1);
				end = buf.GetIterAtLineOffset(loc.EndLine - 1, loc.EndCharacter - 1);
			}
			buf.ApplyTag(tag, start, end);
		}

		/// <summary>
		/// Colorize TextView buffer range corresponding to a <see cref="ParseIssue"/>.
		/// </summary>
		/// <param name="issue">The parse issue.</param>
		protected void ColorizeParseIssue(ParseIssue issue)
		{
			if (object.ReferenceEquals(issue, null))
				throw new ArgumentNullException("issue");

			Location loc = issue.Location;
			if (object.ReferenceEquals(loc, null) || loc.StartLine < 1)
				// No location information known; ignore.
				return;

			TextBuffer buf = this.textviewText.Buffer;
			TextIter start, end;
			TextTag tag;
			if (loc.StartCharacter < 1 && loc.EndCharacter < 1) {
				// Line-wise
				start = buf.GetIterAtLineOffset(loc.StartLine - 1, 0);
				end = buf.GetIterAtLineOffset(loc.EndLine - 1 + 1, 0);
				switch (issue.Severity) {
				case ParseIssueSeverity.Error:
					tag = _TagErrorParseIssueLinewise;
					break;
				case ParseIssueSeverity.Warning:
					tag = _TagWarningParseIssueLinewise;
					break;
				default:
					Debug.Print("[MainWindow] ColorizeParseIssue(): " +
						"Unrecognized severity \"{0}\" line-wise, skipping.",
						issue.Severity);
					return;
				}
			}
			else {
				// Character-wise
				start = buf.GetIterAtLineOffset(loc.StartLine - 1, loc.StartCharacter - 1);
				end = buf.GetIterAtLineOffset(loc.EndLine - 1, loc.EndCharacter - 1);
				switch (issue.Severity) {
				case ParseIssueSeverity.Error:
					tag = _TagErrorParseIssue;
					break;
				case ParseIssueSeverity.Warning:
					tag = _TagWarningParseIssue;
					break;
				default:
					Debug.Print("[MainWindow] ColorizeParseIssue(): " +
						"Unrecognized severity \"{0}\" character-wise, skipping.",
						issue.Severity);
					return;
				}
			}
			buf.ApplyTag(tag, start, end);
		}

		void NodeviewNotes_NodeSelection_Changed(object sender, EventArgs e)
		{
			// Clear state message at the point of a new user interaction.
			// (If node got *de*-selected, rather have a blank status bar
			// than still the information we would have jumped to line <n>.)
			this.statusbar1.Pop(_SbCtxState);

			// Clean up previous "current" colorization. (But keep all other tags.)
			TextBuffer buf = this.textviewText.Buffer;
			buf.RemoveTag(_TagCurrentNotesElement, buf.StartIter, buf.EndIter);
			buf.RemoveTag(_TagCurrentNotesElementLinewise, buf.StartIter, buf.EndIter);

			// Try to retrieve the currently selected node.
			var node = this.nodeviewNotes.NodeSelection.SelectedNode as NotesElementTreeNode;
			if (object.ReferenceEquals(node, null))
				// Keep state.
				return;

			// Scroll TextView to associated position in the text,
			// and tag possibly interesting ranges.
			TextIter start, end;
			switch (node.NodeType) {
			case NotesElementTreeNodeType.NotesElement:
				INotesElement elem = node.NotesElement;
				start = buf.GetIterAtLineOffset(elem.StartLineNumber - 1, 0);
				end = buf.GetIterAtLineOffset(elem.StartLineNumber - 1 + elem.TotalLineCount, 0);

				// Tag entire selected notes element's lines range as current.
				buf.ApplyTag(_TagCurrentNotesElementLinewise, start, end);

				// Remove selection, set cursor to start of element, scroll window to there.
				buf.SelectRange(start, start);
				this.textviewText.ScrollToIter(start, 0, true, 0, 0.5);

				// Inform the user about what has been done.
				this.statusbar1.Push(_SbCtxState,
					string.Format("Jumped to plaintext line {0}.", elem.StartLineNumber));

				break;
			case NotesElementTreeNodeType.ParseIssue:
				if (node.ParseIssue.Location.StartLine < 1) {
					this.statusbar1.Push(_SbCtxState, "Can't jump to parse issue location as none is known.");
					break;
				}

				string locStr;
				if (node.ParseIssue.Location.StartCharacter > 0 &&
				    node.ParseIssue.Location.EndCharacter > 0) {
					start = buf.GetIterAtLineOffset(
						node.ParseIssue.Location.StartLine - 1,
						node.ParseIssue.Location.StartCharacter - 1);
					end = buf.GetIterAtLineOffset(
						node.ParseIssue.Location.EndLine - 1,
						node.ParseIssue.Location.EndCharacter - 1);
					locStr = string.Format("({0},{1})-({2},{3})",
						node.ParseIssue.Location.StartLine,
						node.ParseIssue.Location.StartCharacter,
						node.ParseIssue.Location.EndLine,
						node.ParseIssue.Location.EndCharacter);
				}
				else {
					start = buf.GetIterAtLine(node.ParseIssue.Location.StartLine - 1);
					end = buf.GetIterAtLine(node.ParseIssue.Location.EndLine - 1);
					locStr = string.Format("line {0} to {1} (exclusive)",
						node.ParseIssue.Location.StartLine,
						node.ParseIssue.Location.EndLine);
				}
				buf.SelectRange(start, end);
				this.textviewText.ScrollToIter(start, 0, true, 0, 0.5);

				// Inform the user about what has been done.
				// TODO: More detail? Would need preparing in the previous code part.
				this.statusbar1.Push(_SbCtxState,
					string.Format("Marked parse issue location at {0}.", locStr));

				break;
			default:
				this.statusbar1.Push(_SbCtxState,
					string.Format("Selected node with unknown node type {0}.", node.NodeType));
				break;
			}
		}

		void NodeviewNotes_KeyReleaseEvent(object o, KeyReleaseEventArgs args)
		{
			// Ctrl-F for find, or '/' like in a pager in the terminal.
			if (args.Event.Key == Gdk.Key.slash ||
			    (args.Event.Key == Gdk.Key.f &&
			     args.Event.State == Gdk.ModifierType.ControlMask)) {
				if (object.ReferenceEquals(this.NodeSearchDialog, null)) {
					// Open up a new search dialog.
					this.NodeSearchDialog = new SearchDialog(this);
					this.NodeSearchDialog.Response += (o2, args2) => {
						this.NodeSearchDialog.Destroy();
						this.NodeSearchDialog = null;
					};
					// Position search dialog when it has completely started up.
					GLib.Idle.Add(PositionSearchDialog);
					this.NodeSearchDialog.Show();

					// Hook up search dialog into node view.
					this.nodeviewNotes.SearchColumn = 0;
					this.nodeviewNotes.SearchEntry = this.NodeSearchDialog.EntrySearch;
				}
				else {
					Debug.Print("[MainWindow] NodeviewNotes_KeyReleaseEvent(): " +
						"Trying to give existing search dialog focus, with time {0}...",
						args.Event.Time);
					this.NodeSearchDialog.PresentWithTime(args.Event.Time);
				}
			}
		}

		bool PositionSearchDialog()
		{
			// If we're not dealing with a search dialog currently, ignore.
			if (object.ReferenceEquals(this.NodeSearchDialog, null))
				// Don't call this idle handler again.
				return false;

			// Information about search dialog:
			int dialogWidth, dialogHeight;
			this.NodeSearchDialog.GetSize(out dialogWidth, out dialogHeight);

			// Information about main window:
			int winX, winY;
			this.GetPosition(out winX, out winY);

			int winOriginX, winOriginY;
			this.GdkWindow.GetOrigin(out winOriginX, out winOriginY);

			int winRootOriginX, winRootOriginY;
			this.GdkWindow.GetRootOrigin(out winRootOriginX, out winRootOriginY);

			// Information about NodeView:
			int viewX, viewY, viewWidth, viewHeight, viewDepth;
			this.nodeviewNotes.GdkWindow.GetGeometry(
				out viewX, out viewY, out viewWidth, out viewHeight, out viewDepth);

			// Position search dialog in the top-right corner of the NodeView.
			this.NodeSearchDialog.Move(
				winX + (winOriginX - winRootOriginX) + viewX + viewWidth - dialogWidth,
				winY + (winOriginY - winRootOriginY) + viewY);

			// Don't call this idle handler again.
			return false;
		}
	}
}
