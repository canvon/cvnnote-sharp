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
	/// A semantic type, like type, identifier, constant, keyword, ...
	/// <remarks>
	/// This is to be used with syntax highlighting in the GUI.
	/// </remarks>
	/// </summary>
	public enum SemanticType {
		None = 0,
		Type,
		Identifier,
		Constant,
		Keyword,
	}


	/// <summary>
	/// A wrapped item of type <see cref="T"/> that carries
	/// <see cref="Location"/> and <see cref="SemanticType"/> information.
	/// </summary>
	public class SemanticLocatableItem<T> : SimpleLocatableItem<T>
	{
		/// <summary>
		/// Holds the semantic type information for the associated wrapped <see cref="Item"/>.
		/// </summary>
		/// <value>The semantic type information.</value>
		public SemanticType SemanticType {
			get;
			private set;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CvnNote.SemanticLocatableItem"/> class.
		/// Takes full location information, plus an item to wrap (of type <see cref="T"/>)
		/// and an associated <see cref="SemanticType"/> information.
		/// </summary>
		/// <param name="startLine">Start line, inclusive.</param>
		/// <param name="startCharacter">Start character, inclusive.</param>
		/// <param name="endLine">End line, exclusive.</param>
		/// <param name="endCharacter">End character, exclusive.</param>
		/// <param name="item">Item to wrap.</param>
		/// <param name="semanticType">Semantic type information.</param>
		public SemanticLocatableItem(
			int startLine, int startCharacter,
			int endLine, int endCharacter,
			T item, SemanticType semanticType)
			: base(startLine, startCharacter, endLine, endCharacter, item)
		{
			this.SemanticType = semanticType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CvnNote.SemanticLocatableItem"/> class.
		/// Takes an item to wrap (of type <see cref="T"/>)
		/// and an associated <see cref="SemanticType"/> information.
		/// The location information will be set to
		/// no location information available.
		/// </summary>
		/// <param name="item">Item to wrap.</param>
		/// <param name="semanticType">Semantic type information.</param>
		public SemanticLocatableItem(T item, SemanticType semanticType)
			: this(0, 0, 0, 0, item, semanticType)
		{
		}
	}
}
