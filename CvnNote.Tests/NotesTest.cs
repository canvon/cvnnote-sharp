using NUnit.Framework;
using System;
using System.IO;

namespace CvnNote.Tests
{
	[TestFixture()]
	public class NotesTest
	{
		[Test()]
		public void ParseTest()
		{
			var notes1 = new Notes(new StringReader(
				"2016-09-15\nfoo bar baz\n\tblubber\nquux\n"));

			Assert.Greater(notes1.Days.Count, 0,
				"Days found in parse");
			Assert.AreEqual("2016-09-15", notes1.Days[0].DayIntro.Date,
				"Day intro date");
			Assert.AreEqual(2, notes1.Days[0].DayEntries.Count,
				"Day entries");
			Assert.AreEqual("foo bar baz", notes1.Days[0].DayEntries[0].TypeInformation,
				"Day entry 0 type information");
			Assert.AreEqual("quux", notes1.Days[0].DayEntries[1].TypeInformation,
				"Day entry 1 type information");
		}

		[Test()]
		public void INotesElementTest()
		{
			var notes1 = new Notes(new StringReader(
				"2016-09-15\nsw software1\n\tblubber\nsw software2\nsw software3\n"));

			// Test only the day for now, as this is currently the only class
			// implementing the INotesElement interface!
			INotesElement elem1 = notes1.Days[0];

			Assert.IsNotNull(elem1, "Day 0 as INotesElement");
			Assert.AreEqual("2016-09-15: 3 entries", elem1.PassiveSummary, "Day 0 passive summary");
		}
	}
}
