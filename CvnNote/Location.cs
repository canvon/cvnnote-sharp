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
	/// Stores location information like start line, start character,
	/// end line, end character for an item like a <see cref="ParseIssue"/>.
	/// </summary>
	public class Location
	{
		/// <summary>
		/// Gets the start line, inclusive, of where an item is located.
		/// First line in the input is 1. If 0, this means not known / not set.
		/// </summary>
		/// <value>The start line, inclusive.</value>
		public int StartLine {
			get;
			private set;
		}

		/// <summary>
		/// Gets the start character, inclusive, of where an item is located.
		/// First character in a line is 1. If 0, this means not known/ not set.
		/// </summary>
		/// <value>The start character, inclusive.</value>
		public int StartCharacter {
			get;
			private set;
		}

		/// <summary>
		/// Gets the end line, exclusive, of where an item is located.
		/// Please note that
		/// <c>StartLine == EndLine &amp;&amp; StartCharacter == EndCharacter</c>
		/// means a length of zero (0).
		/// </summary>
		/// <value>The end line, exclusive.</value>
		public int EndLine {
			get;
			private set;
		}

		/// <summary>
		/// Gets the end character, exclusive, of where an item is located.
		/// Please note that
		/// <c>StartLine == EndLine &amp;&amp; StartCharacter == EndCharacter</c>
		/// means a length of zero (0).
		/// </summary>
		/// <value>The end character, exclusive.</value>
		public int EndCharacter {
			get;
			private set;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CvnNote.Location"/> class.
		/// </summary>
		/// <param name="startLine">Start line, inclusive.</param>
		/// <param name="startCharacter">Start character, inclusive.</param>
		/// <param name="endLine">End line, exclusive.</param>
		/// <param name="endCharacter">End character, exclusive.</param>
		public Location(
			int startLine, int startCharacter,
			int endLine, int endCharacter)
		{
			if (startLine < 0)
				throw new ArgumentOutOfRangeException(
					"startLine", startLine,
					"Start line has to be non-negative");

			if (startCharacter < 0)
				throw new ArgumentOutOfRangeException(
					"startCharacter", startCharacter,
					"Start character has to be non-negative");

			if (endLine < 0)
				throw new ArgumentOutOfRangeException(
					"endLine", endLine,
					"End line has to be non-negative");

			if (endCharacter < 0)
				throw new ArgumentOutOfRangeException(
					"endCharacter", endCharacter,
					"End character has to be non-negative");

			if (endLine < startLine)
				throw new ArgumentOutOfRangeException(
					"endLine", endLine,
					"End line can't be smaller than start line");

			if (endLine == startLine && endCharacter < startCharacter)
				throw new ArgumentOutOfRangeException(
					"endCharacter", endCharacter,
					"End character can't be before start character on the same line");

			this.StartLine = startLine;
			this.StartCharacter = startCharacter;
			this.EndLine = endLine;
			this.EndCharacter = endCharacter;
		}


		/// <summary>
		/// Gets the count of lines affected.
		/// This depends on whether there are explicit character numbers
		/// involved or not, and is subject to change, in the search for
		/// a saner number reported to the user.
		/// </summary>
		/// <value>The count of lines affected.</value>
		public int? LineCount {
			get {
				if (this.StartLine == 0)
					return null;

				//if (this.EndLine == this.StartLine)
				//	return this.EndCharacter > this.StartCharacter ? 1 : 0;
				if (this.StartCharacter > 0 && this.EndCharacter > 0)
					return this.EndLine - this.StartLine + 1;
				else
					return this.EndLine - this.StartLine;
			}
		}
	}
}
