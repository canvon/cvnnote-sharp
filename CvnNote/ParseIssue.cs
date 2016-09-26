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
	/// Parse issue severity.
	/// </summary>
	public enum ParseIssueSeverity {
		/// <summary>
		/// Warning means the input could still be parsed,
		/// but possibly under assumptions that will not hold.
		/// </summary>
		Warning,

		/// <summary>
		/// Error means a successful parse was not possible,
		/// e.g., a field could not be converted
		/// to the corresponding data type, or
		/// information was missing.
		/// </summary>
		Error,
	}


	/// <summary>
	/// Stores information about a parse issue,
	/// like location, severity and message.
	///
	/// <remarks>
	/// <seealso cref="Notes">This is to be used
	/// by the <see cref="Notes"/> class.</seealso>
	/// </remarks>
	/// </summary>
	public class ParseIssue : ILocatable
	{
		/// <summary>
		/// Gets the message of the parse issue.
		/// </summary>
		/// <value>The message.</value>
		public string Message {
			get;
			private set;
		}

		/// <summary>
		/// Gets the severity of the parse issue.
		/// </summary>
		/// <value>The severity.</value>
		public ParseIssueSeverity Severity {
			get;
			private set;
		}

		/// <summary>
		/// Gets the <see cref="CvnNote.Location"/> of the parse issue.
		/// </summary>
		/// <value>The location.</value>
		public Location Location {
			get;
			private set;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CvnNote.ParseIssue"/> class.
		/// Takes full location information for where the parse issue is located,
		/// a severity and a format string plus arguments that will produce the message.
		/// </summary>
		/// <param name="startLine">Location's start line, inclusive.</param>
		/// <param name="startCharacter">Location's start character, inclusive.</param>
		/// <param name="endLine">Location's end line, exclusive.</param>
		/// <param name="endCharacter">Location's end character, exclusive.</param>
		/// <param name="severity">Message severity.</param>
		/// <param name="format">Format string.</param>
		/// <param name="args">Formatting arguments.</param>
		public ParseIssue(
			int startLine, int startCharacter,
			int endLine, int endCharacter,
			ParseIssueSeverity severity,
			string format, params object[] args)
		{
			// Checks/assignment for location information.
			this.Location = new Location(startLine, startCharacter, endLine, endCharacter);


			// Checks/assignment for severity.
			this.Severity = severity;


			// Checks for format string and arguments,
			// assignment for message.
			if (object.ReferenceEquals(format, null))
				throw new ArgumentNullException("format");

			// (Try to avoid format string vulnerability.)
			if (args == null || args.Length == 0)
				this.Message = format;
			else
				this.Message = string.Format(format, args);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CvnNote.ParseIssue"/> class.
		/// Takes a severity and a format string plus arguments that will produce the message.
		/// The location information will be set to zeroes (0).
		/// </summary>
		/// <param name="severity">Severity.</param>
		/// <param name="format">Format string.</param>
		/// <param name="args">Formatting arguments.</param>
		public ParseIssue(
			ParseIssueSeverity severity,
			string format, params object[] args)
			: this(0, 0, 0, 0, severity, format, args)
		{
		}
	}
}
