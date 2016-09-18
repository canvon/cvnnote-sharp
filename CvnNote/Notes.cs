﻿using System;
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

				// TODO: Store as DateTime
				public string Date {
					get;
					private set;
				}


				public Intro(IList<string> lines)
				{
					if (object.ReferenceEquals(lines, null))
						throw new ArgumentNullException("lines");

					if (lines.Count != 1)
						throw new ArgumentException("cvnnote day intro has to be a single line", "lines");

					this.TotalLineCount = lines.Count;

					// TODO: Parse into several fields
					// TODO: Convert to DateTime
					Date = lines[0];
				}

				public Intro(IList<string> lines, int startLineNumber)
					: this(lines)
				{
					if (startLineNumber < 0)
						throw new ArgumentOutOfRangeException(
							"startLineNumber", startLineNumber, "Start line number has to be non-negative");

					this.StartLineNumber = startLineNumber;
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

				public string TypeInformation {
					get;
					private set;
				}

				public int BodyLinesCount {
					get;
					private set;
				}


				public Entry(IList<string> lines)
				{
					if (object.ReferenceEquals(lines, null))
						throw new ArgumentNullException("lines");

					if (lines.Count < 1)
						throw new ArgumentException("cvnnote entry has to start with type information", "lines");

					this.TotalLineCount = lines.Count;

					TypeInformation = lines[0];
					// TODO: Process rest of data somehow.
					BodyLinesCount = lines.Count - 1;
				}

				public Entry(IList<string> lines, int startLineNumber)
					: this(lines)
				{
					if (startLineNumber < 0)
						throw new ArgumentOutOfRangeException(
							"startLineNumber", startLineNumber, "Start line number has to be non-negative");

					this.StartLineNumber = startLineNumber;
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

			public Intro DayIntro {
				get;
				private set;
			}

			public IList<Entry> DayEntries {
				get;
				private set;
			}


			public Day(Intro intro, IList<Entry> entries)
			{
				if (object.ReferenceEquals(intro, null))
					throw new ArgumentNullException("intro");

				if (object.ReferenceEquals(entries, null))
					throw new ArgumentNullException("entries");

				// Compute total line count as sum over total line counts
				// of intro and all entries.
				this.TotalLineCount = intro.TotalLineCount;
				foreach (Entry entry in entries) {
					this.TotalLineCount = this.TotalLineCount + entry.TotalLineCount;
				}

				DayIntro = intro;
				DayEntries = entries;
			}

			public Day(Intro intro, IList<Entry> entries, int startLineNumber)
				: this(intro, entries)
			{
				if (startLineNumber < 0)
					throw new ArgumentOutOfRangeException(
						"startLineNumber", startLineNumber, "Start line number has to be non-negative");

				this.StartLineNumber = startLineNumber;
			}

			public Day(IList<string> lines, int startLineNumber)
			{
				if (object.ReferenceEquals(lines, null))
					throw new ArgumentNullException("lines");

				if (startLineNumber < 0)
					throw new ArgumentOutOfRangeException(
						"startLineNumber", startLineNumber, "Start line number has to be non-negative");

				this.StartLineNumber = startLineNumber;
				this.TotalLineCount = lines.Count;

				this.DayIntro = null;
				this.DayEntries = new List<Entry>();

				int subLineNumber = 0, subStartLineNumber = 1;
				var subLines = new List<string>();

				foreach (string line in lines) {
					subLineNumber++;

					if (line.Length == 0) {
						throw new FormatException("Invalid empty line in day parser");
					}
					// Split on non-indented lines.
					else if (subLines.Count > 0 && line[0] != '\t' && line[0] != ' ') {
						if (this.DayIntro == null) {
							this.DayIntro = new Intro(subLines, startLineNumber + subStartLineNumber - 1);
						}
						else {
							this.DayEntries.Add(new Entry(subLines, startLineNumber + subStartLineNumber - 1));
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
					if (this.DayIntro == null)
						this.DayIntro = new Intro(subLines, startLineNumber + subStartLineNumber - 1);
					else
						this.DayEntries.Add(new Entry(subLines, startLineNumber + subStartLineNumber - 1));
				}

				// Post-parse correctness verification:
				if (object.ReferenceEquals(this.DayIntro, null))
					throw new FormatException("Day intro not found");
			}


			public string PassiveSummary {
				get {
					return string.Format("{0}: {1} entries", DayIntro.Date, DayEntries.Count);
				}
			}

			public IList<INotesElement> Children {
				get {
					var ret = new List<INotesElement>();
					ret.Add(DayIntro);
					ret.AddRange(DayEntries);
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

		public IList<Day> Days {
			get;
			private set;
		}


		public Notes(IList<Day> days)
		{
			if (object.ReferenceEquals(days, null))
				throw new ArgumentNullException("days");

			this.TotalLineCount = 0;
			foreach (Day day in days) {
				this.TotalLineCount = this.TotalLineCount + day.TotalLineCount;
			}

			Days = days;
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
					if (lines.Count > 0)
						this.Days.Add(new Day(lines, startLineNumber));

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
			if (lines.Count > 0)
				this.Days.Add(new Day(lines, startLineNumber));

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
