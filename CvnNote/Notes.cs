using System;
using System.Collections.Generic;

namespace CvnNote
{
	public class Notes
	{
		public class Day
		{
			public class Intro
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
			}


			public class Entry
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
	}
}
