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
