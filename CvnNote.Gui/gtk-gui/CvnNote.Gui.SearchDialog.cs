
// This file has been generated by the GUI designer. Do not modify.
namespace CvnNote.Gui
{
	public partial class SearchDialog
	{
		private global::Gtk.HBox hbox2;
		
		private global::Gtk.Label labelSearch;
		
		private global::Gtk.Entry entrySearch;
		
		private global::Gtk.Button buttonCancel;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget CvnNote.Gui.SearchDialog
			this.Name = "CvnNote.Gui.SearchDialog";
			this.Title = global::Mono.Unix.Catalog.GetString ("Search");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.Decorated = false;
			this.DestroyWithParent = true;
			this.SkipTaskbarHint = true;
			// Internal child CvnNote.Gui.SearchDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.labelSearch = new global::Gtk.Label ();
			this.labelSearch.Name = "labelSearch";
			this.labelSearch.LabelProp = global::Mono.Unix.Catalog.GetString ("Search string");
			this.hbox2.Add (this.labelSearch);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.labelSearch]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.entrySearch = new global::Gtk.Entry ();
			this.entrySearch.CanFocus = true;
			this.entrySearch.Name = "entrySearch";
			this.entrySearch.IsEditable = true;
			this.entrySearch.InvisibleChar = '●';
			this.hbox2.Add (this.entrySearch);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.entrySearch]));
			w3.Position = 1;
			w1.Add (this.hbox2);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(w1 [this.hbox2]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Internal child CvnNote.Gui.SearchDialog.ActionArea
			global::Gtk.HButtonBox w5 = this.ActionArea;
			w5.Name = "dialog1_ActionArea";
			w5.Spacing = 10;
			w5.BorderWidth = ((uint)(5));
			w5.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w6 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w5 [this.buttonCancel]));
			w6.Expand = false;
			w6.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w7 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w5 [this.buttonOk]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 400;
			this.DefaultHeight = 300;
			this.Show ();
		}
	}
}
