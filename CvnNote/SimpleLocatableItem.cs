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

namespace CvnNote
{
	/// <summary>
	/// Simple locatable item, a generic class that implements <see cref="ILocatable"/>
	/// and stores / makes available an associated wrapped item in the <see cref="Item"/> property.
	/// </summary>
	public class SimpleLocatableItem<T> : ILocatable
	{
		/// <summary>
		/// Holds the location information for the associated wrapped <see cref="Item"/>.
		/// </summary>
		/// <value>The location information.</value>
		public Location Location {
			get;
			private set;
		}

		/// <summary>
		/// Holds the wrapped item to which the <see cref="Location"/> information applies.
		/// </summary>
		/// <value>The wrapped item.</value>
		public T Item {
			get;
			private set;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CvnNote.SimpleLocatableItem"/> class.
		/// Takes full location information, plus an item to wrap, of type <see cref="T"/>.
		/// </summary>
		/// <param name="startLine">Start line, inclusive.</param>
		/// <param name="startCharacter">Start character, inclusive.</param>
		/// <param name="endLine">End line, exclusive.</param>
		/// <param name="endCharacter">End character, exclusive.</param>
		/// <param name="item">Item to wrap.</param>
		public SimpleLocatableItem(
			int startLine, int startCharacter,
			int endLine, int endCharacter,
			T item)
		{
			this.Location = new Location(startLine, startCharacter, endLine, endCharacter);

			// (Allow null to be stored, think of a "null" literal.)
			//if (object.ReferenceEquals(item, null))
			//	throw new ArgumentNullException("item");

			this.Item = item;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CvnNote.SimpleLocatableItem"/> class.
		/// Takes only an item to wrap, of type <see cref="T"/>;
		/// the location information will be set to
		/// no location information available.
		/// </summary>
		/// <param name="item">Item to wrap.</param>
		public SimpleLocatableItem(T item)
			: this(0, 0, 0, 0, item)
		{
		}
	}
}
