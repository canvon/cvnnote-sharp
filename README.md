
# cvnnote-sharp

__canvon notes in C#/.NET__

My notes, taken via my favourite editor VIM (vi improved) as plain-text files,
follow a rigid formatting. _cvnnote-sharp_ is my second attempt at accessing
my valuable notes files programmatically. That is, it provides a library,
test cases for the library, a command-line interface (CLI) and a graphical
user interface (GUI) which operate on cvnnote files.

The idea this time is: "don't make it too complicated"; rather put something
together that can operate on real notes files now, than to make the perfect
parser (which reads and converts all the contained data in detail; that took
too long to write, and was burdensome to access).

Now I have a GUI which exercises GTK#'s NodeView as a handy foundation, and
"under the hood" an INotesElement interface makes uniformly accessible the
various parts that make up a notes file parse. It doesn't represent all of the
notes file's contents (yet?), but when a node gets selected in the lower half
of the GUI main window, the associated plaintext is shown in the upper half.
When selecting a "parse issue" (i.e., likely a file formatting error), the
plaintext that caused the error/warning gets selected.


## ToC

This README contains: (**Table of Contents**)

  * [Directory layout](#directory-layout), i.e., what can be found where?

  * [Working environment](#working-environment), or what you need to install/run
    to work on these projects.

  * [Contact information](#contact), or who wrote this and from where can it
    be retrieved.


## Directory layout

  * `README.md` - this file. Contains an overview over what this is and how to
    make use of it.

  * `cvnnote-sharp.sln` - the Solution file. Open this in MonoDevelop, or maybe
    Visual Studio or Xamarin Studio, or maybe run xbuild on it. See
    [Working environment](#working-environment).

  * `CvnNote` - contains the library, with data types to represent important
    parts of a cvnnote file's structure, and a means to parse a notes file
    into instances of such data types.

  * `CvnNote.Tests` - contains NUnit unit tests for the library.

  * `CvnNote.Cli` - **CLI** (command-line interface) for the library. Currently
    (2016-09-21/-22), it's only very basic; it reads a notes file and dumps the
    resulting parse to its output again. It tries to represent the resulting
    tree structure, though.

  * `CvnNote.Gui` - **GUI** (graphical user interface) for the library. This is
    the main effort, my notes files have to look good and be accessible in it. 

  * `examples` - contains example data to work on. This contains a `notes`
    subdirectory with example cvnnote files to load into the GUI. Everything
    below `erroneous` is intended to produce parse issues (error messages
    in the GUI). Currently (2016-09-21/-21), a longer example notes file is
    still missing. (I don't want to publish my personal notes files as-is,
    as they may contain information about me or other people that should
    stay private.)


## Working environment
 
I (Fabian) write and run this software under _GNU/Linux_, with _Mono_ as CLI
(Common Language Infrastructure, usually MS .NET on Windows), providing access
to the _GTK_ windowing toolkit library via _GTK#_ ("GTK Sharp"). The IDE
(Integrated Development Environment) I use for cvnnote-sharp is MonoDevelop,
an unbranded version of Xamarin Studio; under Windows, Visual Studio should
also work (if GTK# is installed), as GTK# and GTK are cross-platform, although
the GUI Designer for GTK# (MonoDevelop's _stetic_ GUI Designer) would be
missing, then. The Designer is only required to change the GUI skeleton, though.
It should not be required for simply building or running the application, even
after changes to the source code.

In short: I use MonoDevelop on GNU/Linux. On Windows, Visual Studio should be
an alternative. It should be able to build the GUI and run it, provided GTK#
is installed. For changing the Designer-generated GUI skeleton, you might need
to use Xamarin Studio.

If you don't have access to a proper IDE or don't want to use one, you could
still try to build the projects via Mono's `xbuild` command. The binary to run
will be found in each project directory as `bin/Debug/CvnNote*.exe` (if
applicable).


## Contact

**cvnnote-sharp**, canvon notes in C#/.NET, has been written by:

  * Fabian Pietsch <fabian-cvnnote@canvon.de>  (primary author in 2016)

It is licensed under the GNU General Public License (GPL), version 3 or later.

The project will be hosted on GitHub starting 2016-09-22:

  * https://github.com/canvon/cvnnote-sharp

