using System;
using System.Collections.Generic;
using System.IO;

namespace CvnNote
{
	public interface INotesElement
	{
		int StartLineNumber {
			get;
		}

		int TotalLineCount {
			get;
		}

		string PassiveSummary {
			get;
		}

		IList<INotesElement> Children {
			get;
		}

		IList<ParseIssue> ParseIssues {
			get;
		}

		int TotalParseIssueCount {
			get;
		}
	}


	public class Notes : INotesElement
	{
		public class Day : INotesElement
		{
			public class Intro : INotesElement
			{
				public int StartLineNumber {
					get;
					private set;
				}

				public int TotalLineCount {
					get;
					private set;
				}

				private IList<ParseIssue> _ParseIssues = new List<ParseIssue>();
				public IList<ParseIssue> ParseIssues {
					get {
						return _ParseIssues;
					}
				}

				private void AddParseIssue(ParseIssue issue)
				{
					_ParseIssues.Add(issue);
					TotalParseIssueCount = TotalParseIssueCount + 1;
				}

				public int TotalParseIssueCount {
					get;
					private set;
				}


				// TODO: Store as DateTime
				public string Date {
					get;
					private set;
				}


				public Intro(IList<string> lines, int startLineNumber)
				{
					// Check arguments for formal validity.

					if (object.ReferenceEquals(lines, null))
						throw new ArgumentNullException("lines");

					if (startLineNumber < 0)
						throw new ArgumentOutOfRangeException(
							"startLineNumber", startLineNumber, "Start line number has to be non-negative");

					this.StartLineNumber = startLineNumber;
					this.TotalLineCount = lines.Count;


					// Try parsing the input. From here on, errors get saved
					// but should not produce an exception.

					if (lines.Count < 1) {
						AddParseIssue(new ParseIssue(
							this.StartLineNumber, 0, this.StartLineNumber, 0,
							ParseIssueSeverity.Error,
							"Day intro can't be empty"));
						this.Date = null;
						return;
					}
					else if (lines.Count > 1) {
						int startLine = 0, startCharacter = 0;
						int endLine = 0, endCharacter = 0;
						if (this.StartLineNumber > 0) {
							startLine = this.StartLineNumber;
							endLine = this.StartLineNumber + lines.Count;
						}
						AddParseIssue(new ParseIssue(
							startLine, startCharacter,
							endLine, endCharacter,
							ParseIssueSeverity.Error,
							"Day intro has to be a single line"));
						// Go on and try to parse the first line all the same.
					}

					// TODO: Parse into several fields
					// TODO: Convert to DateTime
					this.Date = lines[0];
				}

				public Intro(IList<string> lines)
					: this(lines, 0)
				{
				}


				public string PassiveSummary {
					get {
						return string.Format("Date {0}", Date);
					}
				}

				public IList<INotesElement> Children {
					get {
						return null;
					}
				}
			}


			public class Entry : INotesElement
			{
				public int StartLineNumber {
					get;
					private set;
				}

				public int TotalLineCount {
					get;
					private set;
				}

				private IList<ParseIssue> _ParseIssues = new List<ParseIssue>();
				public IList<ParseIssue> ParseIssues {
					get {
						return _ParseIssues;
					}
				}

				private void AddParseIssue(ParseIssue issue)
				{
					_ParseIssues.Add(issue);
					TotalParseIssueCount = TotalParseIssueCount + 1;
				}

				public int TotalParseIssueCount {
					get;
					private set;
				}


				public string TypeInformation {
					get;
					private set;
				}

				public int BodyLinesCount {
					get;
					private set;
				}


				public Entry(IList<string> lines, int startLineNumber)
				{
					// Check arguments for formal validity.

					if (object.ReferenceEquals(lines, null))
						throw new ArgumentNullException("lines");

					if (startLineNumber < 0)
						throw new ArgumentOutOfRangeException(
							"startLineNumber", startLineNumber, "Start line number has to be non-negative");

					this.StartLineNumber = startLineNumber;
					this.TotalLineCount = lines.Count;


					// Try parsing the input. From here on, errors get saved
					// but should not produce an exception.

					if (lines.Count < 1) {
						AddParseIssue(new ParseIssue(
							this.StartLineNumber, 0, this.StartLineNumber, 0,
							ParseIssueSeverity.Error,
							"Day entry can't be empty"));
						this.TypeInformation = null;
						this.BodyLinesCount = 0;
						return;
					}
					else if (lines[0][0] == '\t' || lines[0][0] == ' ') {
						int startLine = 0, endLine = 0;
						// If line information is available, ...
						if (this.StartLineNumber > 0) {
							// Mark the whole first line as erroneous.
							startLine = this.StartLineNumber;
							endLine = this.StartLineNumber + 1;
						}
						AddParseIssue(new ParseIssue(
							startLine, 0, endLine, 0,
							ParseIssueSeverity.Error,
							"Day entry has to start with type information"));
						this.TypeInformation = null;
						this.BodyLinesCount = 0;
						return;
					}

					this.TypeInformation = lines[0];
					// TODO: Process rest of data somehow.
					BodyLinesCount = lines.Count - 1;
				}

				public Entry(IList<string> lines)
					: this(lines, 0)
				{
				}


				public string PassiveSummary {
					get {
						return string.Format("Type information {0}, body lines count {1}",
							TypeInformation, BodyLinesCount);
					}
				}

				public IList<INotesElement> Children {
					get {
						var ret = new List<INotesElement>();
						// FIXME: Return actual children.
						return ret;
					}
				}
			}


			public int StartLineNumber {
				get;
				private set;
			}

			public int TotalLineCount {
				get;
				private set;
			}

			private IList<ParseIssue> _ParseIssues = new List<ParseIssue>();
			public IList<ParseIssue> ParseIssues {
				get {
					return _ParseIssues;
				}
			}

			private void AddParseIssue(ParseIssue issue)
			{
				_ParseIssues.Add(issue);
				TotalParseIssueCount = TotalParseIssueCount + 1;
			}

			public int TotalParseIssueCount {
				get;
				private set;
			}


			public Intro DayIntro {
				get;
				private set;
			}

			public IList<Entry> DayEntries {
				get;
				private set;
			}


			public Day(Intro intro, IList<Entry> entries, int startLineNumber)
			{
				if (object.ReferenceEquals(intro, null))
					throw new ArgumentNullException("intro");

				if (object.ReferenceEquals(entries, null))
					throw new ArgumentNullException("entries");

				if (startLineNumber < 0)
					throw new ArgumentOutOfRangeException(
						"startLineNumber", startLineNumber, "Start line number has to be non-negative");

				this.StartLineNumber = startLineNumber;

				// Compute total line/parse_issue count as sum over total line/parse_issue counts
				// of intro and all entries.
				this.TotalLineCount = intro.TotalLineCount;
				this.TotalParseIssueCount = intro.TotalParseIssueCount;
				foreach (Entry entry in entries) {
					this.TotalLineCount = this.TotalLineCount + entry.TotalLineCount;
					this.TotalParseIssueCount = this.TotalParseIssueCount + entry.TotalParseIssueCount;
				}

				this.DayIntro = intro;
				this.DayEntries = entries;
			}

			public Day(Intro intro, IList<Entry> entries)
				: this(intro, entries, 0)
			{
			}

			public Day(IList<string> lines, int startLineNumber)
			{
				// Check arguments for formal validity.

				if (object.ReferenceEquals(lines, null))
					throw new ArgumentNullException("lines");

				if (startLineNumber < 0)
					throw new ArgumentOutOfRangeException(
						"startLineNumber", startLineNumber, "Start line number has to be non-negative");

				this.StartLineNumber = startLineNumber;
				this.TotalLineCount = lines.Count;


				// Try parsing the input. From here on, errors get saved
				// but should not produce an exception.

				this.DayIntro = null;
				this.DayEntries = new List<Entry>();

				int subLineNumber = 0, subStartLineNumber = 1;
				var subLines = new List<string>();

				foreach (string line in lines) {
					subLineNumber++;

					if (line.Length == 0) {
						// This is a formal error of the previous parser,
						// so throw an exception contrary to what was said above.
						throw new FormatException("Invalid empty line in day parser");
					}
					// Split on non-indented lines.
					else if (subLines.Count > 0 && line[0] != '\t' && line[0] != ' ') {
						if (this.DayIntro == null) {
							this.DayIntro = new Intro(subLines, startLineNumber + subStartLineNumber - 1);
							this.TotalParseIssueCount = this.TotalParseIssueCount + this.DayIntro.TotalParseIssueCount;
						}
						else {
							var entry = new Entry(subLines, startLineNumber + subStartLineNumber - 1);
							this.DayEntries.Add(entry);
							this.TotalParseIssueCount = this.TotalParseIssueCount + entry.TotalParseIssueCount;
						}

						// Prepare for more data. (Make sure to keep current line.)
						subStartLineNumber = subLineNumber;
						subLines = new List<string>(new string[]{line});
					}
					else {
						subLines.Add(line);
					}
				}

				// Potentially add a final intro/entry.
				if (subLines.Count > 0) {
					if (this.DayIntro == null) {
						this.DayIntro = new Intro(subLines, startLineNumber + subStartLineNumber - 1);
						this.TotalParseIssueCount = this.TotalParseIssueCount + this.DayIntro.TotalParseIssueCount;
					}
					else {
						var entry = new Entry(subLines, startLineNumber + subStartLineNumber - 1);
						this.DayEntries.Add(entry);
						this.TotalParseIssueCount = this.TotalParseIssueCount + entry.TotalParseIssueCount;
					}
				}

				// Post-parse correctness verification:
				if (object.ReferenceEquals(this.DayIntro, null)) {
					AddParseIssue(new ParseIssue(
						startLineNumber, 0, startLineNumber + subStartLineNumber, 0,
						ParseIssueSeverity.Error,
						"Day intro not found"));
				}
			}


			public string PassiveSummary {
				get {
					return string.Format(
						"{0}: {1} entries",
						this.DayIntro != null ? this.DayIntro.Date : "(Unknown)",
						this.DayEntries.Count);
				}
			}

			public IList<INotesElement> Children {
				get {
					var ret = new List<INotesElement>();
					if (!object.ReferenceEquals(this.DayIntro, null))
						ret.Add(this.DayIntro);
					ret.AddRange(this.DayEntries);
					return ret;
				}
			}
		}


		public int StartLineNumber {
			get {
				// As long as the notes always start the start of file,
				// this will be constant 1.
				return 1;
			}
		}

		public int TotalLineCount {
			get;
			private set;
		}

		private IList<ParseIssue> _ParseIssues = new List<ParseIssue>();
		public IList<ParseIssue> ParseIssues {
			get {
				return _ParseIssues;
			}
		}

		private void AddParseIssue(ParseIssue issue)
		{
			_ParseIssues.Add(issue);
			TotalParseIssueCount = TotalParseIssueCount + 1;
		}

		public int TotalParseIssueCount {
			get;
			private set;
		}


		public IList<Day> Days {
			get;
			private set;
		}


		public Notes(IList<Day> days)
		{
			if (object.ReferenceEquals(days, null))
				throw new ArgumentNullException("days");

			this.TotalLineCount = 0;
			this.TotalParseIssueCount = 0;
			foreach (Day day in days) {
				this.TotalLineCount = this.TotalLineCount + day.TotalLineCount;
				this.TotalParseIssueCount = this.TotalParseIssueCount + day.TotalParseIssueCount;
			}

			this.Days = days;
		}

		public Notes(TextReader reader)
		{
			if (object.ReferenceEquals(reader, null))
				throw new ArgumentNullException("reader");

			this.Days = new List<Day>();
			IList<string> lines = new List<string>();
			string line;
			int lineNumber = 0, startLineNumber = 1;

			while ((line = reader.ReadLine()) != null) {
				lineNumber++;

				// Split on empty lines.
				if (line.Length == 0) {
					if (lines.Count > 0) {
						var day = new Day(lines, startLineNumber);
						this.Days.Add(day);
						this.TotalParseIssueCount += this.TotalParseIssueCount + day.TotalParseIssueCount;
					}

					// Prepare for more data.
					// (But skip current line.)
					startLineNumber = lineNumber + 1;
					lines = new List<string>();
				}
				else {
					lines.Add(line);
				}
			}

			// Potentially add a final Day.
			if (lines.Count > 0) {
				var day = new Day(lines, startLineNumber);
				this.Days.Add(day);
				this.TotalParseIssueCount += this.TotalParseIssueCount + day.TotalParseIssueCount;
			}

			this.TotalLineCount = lineNumber;
		}


		public string PassiveSummary {
			get {
				return string.Format("Notes with {0} days", Days.Count);
			}
		}

		public IList<INotesElement> Children {
			get {
				var ret = new List<INotesElement>();
				ret.AddRange(Days);
				return ret;
			}
		}
	}
}
