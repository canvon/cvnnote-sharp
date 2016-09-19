using System;
using Gtk;

namespace CvnNote.Gui
{
	public partial class SearchDialog : Gtk.Dialog
	{
		public SearchDialog(Window parent)
			: base("Search - cvnnote-sharp GUI", parent, DialogFlags.DestroyWithParent)
		{
			this.Build();
		}


		public Entry EntrySearch {
			get {
				return this.entrySearch;
			}
		}
	}
}
