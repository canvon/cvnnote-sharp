using NUnit.Framework;
using System;

namespace CvnNote.Tests
{
	[TestFixture()]
	public class NotesTest
	{
		[Test()]
		public void ParseTest()
		{
			var notes1 = new Notes(new System.IO.StringReader(
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
	}
}
