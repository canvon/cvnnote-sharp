﻿/*
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

		IList<ISemanticLocatable> SyntaxElements {
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


				public bool IsSkip {
					get;
					private set;
				}

				public SemanticLocatableItem<string> Skip {
					get;
					private set;
				}

				public static readonly string SkipString = "[...]";

				public SemanticLocatableItem<DateTime> Date {
					get;
					private set;
				}

				public static readonly string DateFormat = "yyyy'-'MM'-'dd";

				public string DateFormatted {
					get {
						if (this.IsSkip)
							return SkipString;
						return !object.ReferenceEquals(this.Date, null) ?
							string.Format("{0:" + DateFormat + "}", this.Date.Item) :
							"(date missing)";
					}
				}

				public SemanticLocatableItem<int> Chapter {
					get;
					private set;
				}

				public string ChapterFormattedForAppend {
					get {
						int chapter = 0;
						if (!object.ReferenceEquals(this.Chapter, null))
							chapter = this.Chapter.Item;
						return chapter == 1 ? String.Empty : string.Format(" ({0})", chapter);
					}
				}

				public SemanticLocatableItem<string> Comment {
					get;
					private set;
				}

				public string CommentFormattedForAppend {
					get {
						if (object.ReferenceEquals(this.Comment, null))
							return string.Empty;
						if (object.ReferenceEquals(this.Comment.Item, null))
							return string.Empty;
						return string.Format(" ({0})", this.Comment.Item);
					}
				}

				public string IntroFormatted {
					get {
						return
							this.DateFormatted +
							this.ChapterFormattedForAppend +
							this.CommentFormattedForAppend;
					}
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

					this.IsSkip = false;
					this.Skip = null;
					this.Date = null;
					this.Chapter = null;
					this.Comment = null;

					if (lines.Count < 1) {
						AddParseIssue(new ParseIssue(
							this.StartLineNumber, 0, this.StartLineNumber, 0,
							ParseIssueSeverity.Error,
							"Day intro can't be empty"));
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

					string[] fields = lines[0].Split(new char[]{' '}, 3);
					string   field;
					int prevLen = 0;
					if (fields.Length == 0) {
						AddParseIssue(new ParseIssue(
							this.StartLineNumber, 0, this.StartLineNumber, 0,
							ParseIssueSeverity.Error,
							"Day intro missing date part"));
						return;
					}

					field = fields[0];
					try {
						if (field == SkipString) {
							this.IsSkip = true;
							this.Skip = new SemanticLocatableItem<string>(
								this.StartLineNumber, 1, this.StartLineNumber, 1 + SkipString.Length,
								SkipString, SemanticType.Constant);
						}
						else {
							DateTime date = DateTime.ParseExact(field, DateFormat, null);
							this.Date = new SemanticLocatableItem<DateTime>(
								this.StartLineNumber, 1, this.StartLineNumber, 1 + field.Length,
								date, SemanticType.Constant);
						}
					}
					catch (Exception ex) {
						AddParseIssue(new ParseIssue(
							this.StartLineNumber, 1, this.StartLineNumber, 1 + field.Length,
							ParseIssueSeverity.Error,
							"Day intro has invalid date \"{0}\": {1}", field, ex.Message));
						// Go on parsing...
					}
					prevLen = prevLen + field.Length + 1;  // Previous length + field length + separator.

					if (fields.Length >= 2) {
						field = fields[1];
						try {
							if (field.Length < 2)
								throw new FormatException("Chapter number must be enclosed in parentheses");

							if (field[0] != '(')
								throw new FormatException("Chapter opening parenthesis must be '('");

							if (field[field.Length - 1] != ')')
								throw new FormatException("Chapter closing parenthesis must be ')'");

							string chapterString = field.Substring(1, field.Length - 2);
							int chapter = int.Parse(chapterString);
							this.Chapter = new SemanticLocatableItem<int>(
								this.StartLineNumber, 1 + prevLen,
								this.StartLineNumber, 1 + prevLen + field.Length,
								chapter, SemanticType.Constant);
						}
						catch (Exception ex) {
							AddParseIssue(new ParseIssue(
								this.StartLineNumber, 1 + prevLen,
								this.StartLineNumber, 1 + prevLen + field.Length,
								ParseIssueSeverity.Warning,
								"Day intro has unrecognized chapter number \"{0}\": {1}",
								field, ex.Message));
							// Go on parsing...
						}
						prevLen = prevLen + field.Length + 1;  // Previous length + field length + separator.
					}
					else {
						// Default to 1.
						// (But only if we got to parse it at all!
						// That is, stays 0 on severe errors.)
						this.Chapter = new SemanticLocatableItem<int>(1, SemanticType.None);
					}

					if (fields.Length >= 3) {
						field = fields[2];
						try {
							if (field.Length < 2)
								throw new FormatException("Comment must be enclosed in parentheses");

							if (field[0] != '(')
								throw new FormatException("Comment opening parenthesis must be '('");

							if (field[field.Length - 1] != ')')
								throw new FormatException("Comment closing parenthesis must be ')'");

							string comment = field.Substring(1, field.Length - 2);
							this.Comment = new SemanticLocatableItem<string>(
								this.StartLineNumber, 1 + prevLen,
								this.StartLineNumber, 1 + prevLen + field.Length,
								comment, SemanticType.Identifier);
						}
						catch (Exception ex) {
							AddParseIssue(new ParseIssue(
								this.StartLineNumber, 1 + prevLen,
								this.StartLineNumber, 1 + prevLen + field.Length,
								ParseIssueSeverity.Warning,
								"Day intro has unrecognized comment \"{0}\": {1}",
								field, ex.Message));
							// Go on parsing...
						}
						//prevLen = prevLen + field.Length + 1;  // Previous length + field length + separator.
					}

					// Done.
				}

				public Intro(IList<string> lines)
					: this(lines, 0)
				{
				}


				public string PassiveSummary {
					get {
						string labelling = this.IsSkip ? "Skip" : "Date {0}";

						return string.Format(labelling + ", chapter {1}, comment: {2}",
							!object.ReferenceEquals(this.Date, null) ?
								string.Format("{0:D}", this.Date.Item) :
								"(missing)",
							!object.ReferenceEquals(this.Chapter, null) ?
								this.Chapter.Item :
								0,
							this.Comment?.Item ?? "(none)");
					}
				}

				public IList<INotesElement> Children {
					get {
						return null;
					}
				}

				public IList<ISemanticLocatable> SyntaxElements {
					get {
						var ret = new List<ISemanticLocatable>();

						if (this.IsSkip) {
							ret.Add(this.Skip);
						}
						else {
							if (!object.ReferenceEquals(this.Date, null))
								ret.Add(this.Date);
							if (!object.ReferenceEquals(this.Chapter, null))
								ret.Add(this.Chapter);
							if (!object.ReferenceEquals(this.Comment, null))
								ret.Add(this.Comment);
						}

						return ret;
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


				/// <summary>
				/// Holds the first part of an entry's first line.
				/// This will usually be a category, like
				/// <c>adm</c> (administration) or <c>develop</c> (software development)
				/// or something on that lines.
				/// </summary>
				/// <value>The category / first part of entry first line.</value>
				public SemanticLocatableItem<string> Category {
					get;
					private set;
				}

				/// <summary>
				/// Holds the rest of an entry's first line.
				/// This will usually be additional / more specific information, like
				/// <c>luna</c> (a hostname) or <c>cvnnote-sharp</c> (a project name).
				/// But it can be anything that makes the <see cref="Category"/>
				/// more specific -- a complement.
				/// </summary>
				/// <value>The complement / rest of entry first line.</value>
				public SemanticLocatableItem<string> Complement {
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

					this.Category = null;
					this.Complement = null;
					this.BodyLinesCount = 0;

					if (lines.Count < 1) {
						AddParseIssue(new ParseIssue(
							this.StartLineNumber, 0, this.StartLineNumber, 0,
							ParseIssueSeverity.Error,
							"Day entry can't be empty"));
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
							"Day entry has to start with a category name"));
						return;
					}

					string[] fields = lines[0].Split(new char[]{' '}, 2);
					string field;
					int prevLen = 0;
					if (object.ReferenceEquals(fields, null) ||
					    fields.Length < 1 ||
					    string.IsNullOrWhiteSpace(fields[0])) {

						int startLine = 0, endLine = 0;
						if (this.StartLineNumber > 0) {
							startLine = this.StartLineNumber;
							endLine = this.StartLineNumber + 1;
						}
						AddParseIssue(new ParseIssue(
							startLine, 0, endLine, 0,
							ParseIssueSeverity.Error,
							"Day entry category/complement unexpectedly missing"));
						return;
					}

					// Save category string with associated location and semantic information.
					field = fields[0];
					this.Category = new SemanticLocatableItem<string>(
						this.StartLineNumber, 1, this.StartLineNumber, 1 + field.Length,
						field, SemanticType.Type);
					prevLen = prevLen + field.Length + 1;

					// TODO: Further parse complement?
					if (fields.Length >= 2 && !string.IsNullOrWhiteSpace(fields[1])) {
						// Save complement string with associated location and semantic information.
						field = fields[1];
						this.Complement = new SemanticLocatableItem<string>(
							this.StartLineNumber, 1 + prevLen,
							this.StartLineNumber, 1 + prevLen + field.Length,
							field.Trim(), SemanticType.Identifier);
					}
					//prevLen = prevLen + 1 + field.Length;

					if (fields.Length > 2) {
						int startCharacter = fields[0].Length + 1 + fields[1].Length + 1;
						AddParseIssue(new ParseIssue(
							this.StartLineNumber, startCharacter,
							this.StartLineNumber, lines[0].Length + 1,
							ParseIssueSeverity.Warning,
							"Trailing unrecognized data after day entry category/complement: {0} additional fields",
							fields.Length - 2));
						// Ignore / go on parsing...
					}

					// TODO: Process rest of data somehow.
					BodyLinesCount = lines.Count - 1;
				}

				public Entry(IList<string> lines)
					: this(lines, 0)
				{
				}


				public string PassiveSummary {
					get {
						return string.Format(
							"Category {0}{1}; body lines count {2}",
							this.Category?.Item ?? "(unknown)",
							!object.ReferenceEquals(this.Complement?.Item, null) ?
								string.Format(" / \"{0}\"", this.Complement.Item) :
								string.Empty,
							this.BodyLinesCount);
					}
				}

				public IList<INotesElement> Children {
					get {
						var ret = new List<INotesElement>();
						// FIXME: Return actual children.
						return ret;
					}
				}

				public IList<ISemanticLocatable> SyntaxElements {
					get {
						var ret = new List<ISemanticLocatable>();

						if (!object.ReferenceEquals(this.Category, null))
							ret.Add(this.Category);
						if (!object.ReferenceEquals(this.Complement, null))
							ret.Add(this.Complement);

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
						this.DayIntro != null ? this.DayIntro.IntroFormatted : "(Unknown)",
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

			public IList<ISemanticLocatable> SyntaxElements {
				get {
					return null;
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
			this.TotalParseIssueCount = 0;

			while ((line = reader.ReadLine()) != null) {
				lineNumber++;

				// Split on empty lines.
				if (line.Length == 0) {
					if (lines.Count > 0) {
						var day = new Day(lines, startLineNumber);
						this.Days.Add(day);
						this.TotalParseIssueCount = this.TotalParseIssueCount + day.TotalParseIssueCount;
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
				this.TotalParseIssueCount = this.TotalParseIssueCount + day.TotalParseIssueCount;
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

		public IList<ISemanticLocatable> SyntaxElements {
			get {
				return null;
			}
		}
	}
}
