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
			Assert.AreEqual("Date 2016-09-15", elem1Children[0].PassiveSummary,
				"Day 0 child 0 (Notes.Day.Intro)");
			Assert.AreEqual("Type information sw software1", elem1Children[1].PassiveSummary,
				"Day 0 child 1 (Notes.Day.Entry, software1");
			Assert.AreEqual("Type information sw software2", elem1Children[2].PassiveSummary,
				"Day 0 child 2 (Notes.Day.Entry, software2");
			Assert.AreEqual("Type information sw software3", elem1Children[3].PassiveSummary,
				"Day 0 child 3 (Notes.Day.Entry, software3");
		}
	}
}
