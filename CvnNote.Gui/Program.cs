using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gtk;

namespace CvnNote.Gui
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			// (This has to happen early, for e.g. glib error printing
			// to contain the correct program name.)
			// TODO: Consider using the overload with parameters?
			//       Is it even necessary so that the standard GTK
			//       command line options will work?
			//       (But where to get the program name argument from...)
			Application.Init();

			GLib.ExceptionManager.UnhandledException += HandleUnhandledException;


			var log = new GLib.Log();
			var filePaths = new List<string>();

			// Parse command-line arguments.
			for (int i = 0; i < args.Length; i++) {
				switch (args[i]) {
				case "--debug":
					#if DEBUG
					Debug.Listeners.Add(new ConsoleTraceListener());
					Debug.Print("[MainClass] Debugging output to terminal enabled.");
					#else
					log.WriteLog(null, GLib.LogLevelFlags.Critical,
						"Debug mode only supported on Debug builds!");
					return 2;
					#endif
					break;
				default:
					if (args[i].Length >= 1 && args[i][0] == '-') {
						log.WriteLog(null, GLib.LogLevelFlags.Critical,
							"Invalid option argument \"{0}\"",
							args[i]);
						return 2;
					}
					else {
						Debug.Print("[MainClass] Remembering file path \"{0}\" for opening...", args[i]);
						filePaths.Add(args[i]);
					}
					break;
				}
			}

			// Final syntax checks.
			// TODO: Remove, when multi-file support has been added.
			if (filePaths.Count > 1) {
				log.WriteLog(null, GLib.LogLevelFlags.Critical,
					"Can't open {0} files: More than one file to open not supported, yet!",
					filePaths.Count);
				return 2;
			}


			MainWindow win = new MainWindow();
			win.Show();

			// Get app up before trying to open files in it.
			while (Application.EventsPending())
				Application.RunIteration(true);

			// Open files in the started application.
			// TODO: Support opening more files at once.
			if (filePaths.Count > 0) {
				Debug.Print("[MainClass] Opening file \"{0}\" in MainWindow...",
					filePaths[0]);
				win.LoadFile(filePaths[0]);
			}

			// Continue running normally.
			Application.Run();

			return 0;
		}

		static void HandleUnhandledException(GLib.UnhandledExceptionArgs args)
		{
			var log = new GLib.Log();

			string exObjFullName = args.ExceptionObject.GetType().FullName;

			var ex = args.ExceptionObject as Exception;
			string exMsg = object.ReferenceEquals(ex, null) ?
				"(Exception object is not an Exception)" :
				ex.Message;

			log.WriteLog(null, GLib.LogLevelFlags.Critical,
				"Caught unhandled exception of type {0}: {1}",
				exObjFullName, exMsg);
			Debug.Print("Stack trace:{0}{1}", Environment.NewLine, Environment.StackTrace);

			var dialog = new MessageDialog(
				null, DialogFlags.Modal, MessageType.Error, ButtonsType.OkCancel,
				"Caught unhandled exception of type {0}: {1}" +
				Environment.NewLine + Environment.NewLine +
				"Terminate? (Hit cancel to try to continue...)",
				exObjFullName, exMsg);
			int result = dialog.Run();
			dialog.Destroy();

			if (result == (int)ResponseType.Ok) {
				log.WriteLog(null, GLib.LogLevelFlags.Warning, "User decided to terminate the application.");
				args.ExitApplication = true;
			}
			else {
				log.WriteLog(null, GLib.LogLevelFlags.Warning, "Trying to continue on user request...");
				args.ExitApplication = false;
			}
		}
	}
}
