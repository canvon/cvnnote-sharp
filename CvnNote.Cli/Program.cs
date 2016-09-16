using System;
using System.Collections.Generic;
using System.IO;

namespace CvnNote.Cli
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			var inputFilePaths = new List<string>();

			for (int i = 0; i < args.Length; i++) {
				if (args[i].Length >= 1 && args[i][0] == '-') {
					Console.Error.WriteLine(
						"Invalid argument \"{0}\": No command-line options supported, yet.",
						args[i]);
					return 2;
				}
				else {
					inputFilePaths.Add(args[i]);
				}
			}

			// On no arguments given, try to read notes file data
			// from standard input.
			if (inputFilePaths.Count == 0)
				inputFilePaths.Add("/dev/stdin");  // TODO: Make portable.

			foreach (string inputFilePath in inputFilePaths) {
				if (inputFilePaths.Count > 1)
					Console.WriteLine("{0}:", inputFilePath);

				try {
					using (TextReader reader = new StreamReader(inputFilePath)) {
						var notes = new Notes(reader);

						DumpNotesTree(notes);
					}
				}
				catch (IOException ex) {
					Console.Error.WriteLine(
						"Error processing file \"{0}\": {1}",
						inputFilePath, ex.Message);
					return 1;
				}
				catch (Exception ex) {
					Console.Error.WriteLine(
						"Unexpected error (input file was \"{0}\"): {1}",
						inputFilePath, ex.Message);
					return 1;
				}
			}

			return 0;
		}

		public static void DumpNotesTree(INotesElement tree)
		{
			DumpNotesTree(tree, 0);
		}

		public static void DumpNotesTree(INotesElement tree, int indent)
		{
			if (object.ReferenceEquals(tree, null))
				throw new ArgumentNullException("tree");

			if (indent < 0)
				throw new ArgumentOutOfRangeException("indent", indent, "Indent has to be non-negative");

			string indentStr = String.Empty;
			for (int i = 0; i < indent; i++)
				indentStr += "  ";

			Console.WriteLine("{0}{1}", indentStr, tree.PassiveSummary);

			IList<INotesElement> childs = tree.Children;
			if (childs != null) {
				foreach (INotesElement child in childs) {
					DumpNotesTree(child, indent + 1);
				}
			}
		}
	}
}
