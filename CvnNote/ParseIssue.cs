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
	public class ParseIssue
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
		/// Gets the start line, inclusive, of where the parse issue is located.
		/// First line in the input is 1. If 0, this means not known / not set.
		/// </summary>
		/// <value>The start line, inclusive.</value>
		public int StartLine {
			get;
			private set;
		}

		/// <summary>
		/// Gets the start character, inclusive, of where the parse issue is located.
		/// First character in a line is 1. If 0, this means not known/ not set.
		/// </summary>
		/// <value>The start character, inclusive.</value>
		public int StartCharacter {
			get;
			private set;
		}

		/// <summary>
		/// Gets the end line, exclusive, of where the parse issue is located.
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
		/// Gets the end character, exclusive, of where the parse issue is located.
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
		/// Initializes a new instance of the <see cref="CvnNote.ParseIssue"/> class.
		/// Takes full location information for where the parse issue is located,
		/// a severity and a format string plus arguments that will produce the message.
		/// </summary>
		/// <param name="startLine">Location's start line.</param>
		/// <param name="startCharacter">Location's start character.</param>
		/// <param name="endLine">Location's end line.</param>
		/// <param name="endCharacter">Location's end character.</param>
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
