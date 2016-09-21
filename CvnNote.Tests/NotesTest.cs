using NUnit.Framework;
using System;
using System.Collections.Generic;
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
			Assert.AreEqual("2016-09-15", notes1.Days[0].DayIntro.DateFormatted,
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

			INotesElement elem0 = notes1;

			Assert.IsNotNull(elem0,
				"Notes parse as INotesElement");
			Assert.AreEqual("Notes with 1 days", elem0.PassiveSummary,
				"Notes parse passive summary");

			IList<INotesElement> elem0Children = elem0.Children;
			Assert.AreEqual(1, elem0Children.Count,
				"Notes parse children");
			INotesElement elem1 = elem0Children[0];

			Assert.IsNotNull(elem1,
				"Day 0 as INotesElement");
			Assert.AreEqual("2016-09-15: 3 entries", elem1.PassiveSummary,
				"Day 0 passive summary");

			IList<INotesElement> elem1Children = elem1.Children;
			Assert.AreEqual(4, elem1Children.Count,
				"Day 0 children");
			Assert.AreEqual("Date 15 September 2016, chapter 1, comment: (none)", elem1Children[0].PassiveSummary,
				"Day 0 child 0 (Notes.Day.Intro)");
			Assert.AreEqual("Type information sw software1, body lines count 1", elem1Children[1].PassiveSummary,
				"Day 0 child 1 (Notes.Day.Entry, software1)");
			Assert.AreEqual("Type information sw software2, body lines count 0", elem1Children[2].PassiveSummary,
				"Day 0 child 2 (Notes.Day.Entry, software2)");
			Assert.AreEqual("Type information sw software3, body lines count 0", elem1Children[3].PassiveSummary,
				"Day 0 child 3 (Notes.Day.Entry, software3)");
		}

		[Test()]
		public void TotalLineCountTest()
		{
			var notes1 = new Notes(new StringReader(
				"2016-09-18\ndevelop project A\n\t* Some notes.\n\t* Some more ontes.\n\t* Even more notes.\n" +
				"sw something\n\tSome notes on that software.\n" +
				"develop project B\n\tSome notes on another development project.\n"));

			Assert.AreEqual(1, notes1.Days[0].DayIntro.TotalLineCount,
				"Day 0 intro total line count");
			Assert.AreEqual(4, notes1.Days[0].DayEntries[0].TotalLineCount,
				"Entry 0 total line count");
			Assert.AreEqual(2, notes1.Days[0].DayEntries[1].TotalLineCount,
				"Entry 1 total line count");
			Assert.AreEqual(2, notes1.Days[0].DayEntries[2].TotalLineCount,
				"Entry 2 total line count");

			Assert.AreEqual(9, notes1.Days[0].TotalLineCount,
				"Day 0 total line count");
			Assert.AreEqual(9, notes1.TotalLineCount,
				"Notes total line count");
		}
	}
}
