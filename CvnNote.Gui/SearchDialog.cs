using System;
using Gtk;

namespace CvnNote.Gui
{
	public partial class SearchDialog : Gtk.Dialog
	{
		public SearchDialog()
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
