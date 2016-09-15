using System;
using System.Collections.Generic;
using System.IO;

namespace CvnNote
{
	public interface INotesElement
	{
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

					// TODO: Parse into several fields
					// TODO: Convert to DateTime
					Date = lines[0];
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
				public string TypeInformation {
					get;
					private set;
				}


				public Entry(IList<string> lines)
				{
					if (object.ReferenceEquals(lines, null))
						throw new ArgumentNullException("lines");

					if (lines.Count < 1)
						throw new ArgumentException("cvnnote entry has to start with type information", "lines");

					TypeInformation = lines[0];
					// TODO: Process rest of data somehow.
				}


				public string PassiveSummary {
					get {
						// TODO: Something like: return string.Format("Entry with {0} lines", lines.Count);
						return string.Format("Type information {0}", TypeInformation);
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

				DayIntro = intro;
				DayEntries = entries;
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


		public IList<Day> Days {
			get;
			private set;
		}


		public Notes(IList<Day> days)
		{
			if (object.ReferenceEquals(days, null))
				throw new ArgumentNullException("days");

			Days = days;
		}

		public Notes(TextReader reader)
		{
			if (object.ReferenceEquals(reader, null))
				throw new ArgumentNullException("reader");

			Days = new List<Day>();
			Day.Intro intro = null;
			IList<Day.Entry> entries = new List<Day.Entry>();
			IList<string> lines = new List<string>();
			string line;

			while ((line = reader.ReadLine()) != null) {
				if (line.Length == 0) {
					if (intro != null) {
						if (lines.Count > 0) {
							entries.Add(new Day.Entry(lines));
							lines = new List<string>();
						}

						Days.Add(new Day(intro, entries));
						intro = null;
						entries = new List<Day.Entry>();
					}
				}
				else if (lines.Count > 0 && line[0] != '\t' && line[0] != ' ') {
					if (intro == null) {
						intro = new Day.Intro(lines);
					}
					else {
						entries.Add(new Day.Entry(lines));
					}

					lines = new List<string>(new string[]{line});
				}
				else {
					lines.Add(line);
				}
			}

			// Potentially add a final Day.
			if (intro != null) {
				if (lines.Count > 0)
					entries.Add(new Day.Entry(lines));

				Days.Add(new Day(intro, entries));
			}
			else {
				if (lines.Count > 0)
					throw new FormatException("Unrecognized lines left");
			}
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
