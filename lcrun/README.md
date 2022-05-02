# lcrun - CLI application bootstrapper

> :warning: This is a work in progress

## Background

I tend to craft many command line applications, most commonly as
.NET applications (like lcrun.exe itself). Increasingly I run into the
problem that I want all those little applications to be available directly
on the commandline. Well, "that is simple", you'd say. There are multiple
ways to do so.

* Option 1: Just copy them all to a single folder that is on the PATH.
The problem: especially .NET applications tend to come with dependencies
and those can cause conflicts if you just copy the application folders
of multiple applications over each other. So ... that doesn't really
work anymore these days.

* Option 2: Put each of the application directories on the PATH. Well ...
with a large number of applications that leads to a very long PATH
variable, possibly reaching its maximum size. So ... that doesn't really
work either.

* Option 3: Invoke each command via its full path. Not really feasible
for a CLI interface that uses many different applications

* Option 4: Have a central directory that contains symbolic links for
all the applications. On a UNIX system that actually is a great solution.
On Windows not so much: the only way to create symbolic links to files
requires (by default) administrator elevation, making it a bad user
experience.

* Option 5: Use a bootstrapper application. That is an application that
is on the path and that knows how to start other applications and pass
arguments providing a minimal layer between the command shell and the
actual application. That is what "lcrun" provides. And once you have a
layer between the shell and the actual application you want to execute,
you can use that same mechanism to tweak the application invocation,
for instance modify the PATH or other environment variables, or
prepend or append extra arguments.

## How it works

Say, for example, that you built a commandline application named
"myapp.exe". However, your PATH is already kindof full and you don't
want to put it directly on the PATH. Nor do you want to remember its
exact full path.

Now you want to run it with three arguments, "arg1 arg2 arg3". You
could run it using the full path, something like:

```bat
C:\src\mysln\myapp\bin\debug\net6\myapp.exe arg1 arg2 arg3
```

That quickly gets tedious. Instead, you can create an
_application definition file_ for "myapp", and run your app with
_lcrun_ as

```bat
lcrun myapp arg1 arg2 arg3
```

Using your application definition file, lcrun can find where
myapp.exe actually is located, can tweak the run environment
(by default: put the application's directory at the start of PATH)
and then execute myapp.exe and pass the arguments to it.

## Usage

The basic invocation of _lcrun_ looks as follows:

> lcrun _[options]_ _app_name_ _app_arguments_

* _[options]_ are zero or more options that affect the behaviour
of _lcrun_ itself. Normally there are none, but they can be useful
when troubleshooting.
* _app_name_ is the name of the application to run, identifying the
application definition file that describes the application. 
(Application definition files are described later in this document.)
* _app_arguments_ are the arguments to the application

_lcrun_ also supports a few _commands_ for managing your application
definition files. These use a command name starting with '/' instead
of an _app_name_. To get a list of commands supported by the current
version of _lcrun_, just run _lcrun_ without any arguments.

```bat
> lcrun

Application bootstrapper utility.
General synopsis is one of:
  lcrun <-v> <apptag> <arguments>
  lcrun <-v> /<command> <command-specific-options>

lcrun [-v] [-dry] [-dmp] <apptag> <arguments>
  Runs the application identified by <apptag> with the given
  arguments.
lcrun /help [/<command>]
  Print a more detailed version of this help message or the
  command's help message
lcrun [-v] /list [-q|-b|-v] {-m <txt>}
  List the registered applications
lcrun /show <apptag>
  Print the application definition (and base definitions)
lcrun [-v] /register ...
  (use 'lcrun /help register' or 'lcrun /register -h' for full
  help) Create a new (stub) appdef file. Consider further tuning
  in a text editor.
```
### _lcrun_ options

_lcrun_ currently supports the following options

* -v : enables verbose mode: printing status information to STDERR and
providing more detailed exception info
* -dry : enables "dry run" mode. In this mode _lcrun_ parses and
prepares the command to execute, but stops before actually executing it
* -dmp : after preparing the command to execute, a JSON file describing
the full execution context is dumped to a file

### _lcrun_ commands

For the most up-to-date description of the commands, run _lcrun_
without any arguments.

#### _lcrun /list_

Print a listing of the registered applications. This listing
can be in one of 3 modes: quiet, brief or verbose

```bat
lcrun [-v] /list [-q|-b|-v] {-m <txt>}
```

* -v (before '/list') : general lcrun verbosity flag
* -q : quiet listing. Just print names, does not load the appdefs,
so this mode is resilient against errors in appdef files
* -b : brief listing. This is the default mode. This lists the names,
base, and command
* -v (after '/list') : verbose listing. This also lists the description
of the commands.
* -m \<text> (repeatable) : only list applications matching the given
text (or any of the given -m texts if multiple are given). If no -m
options are given, all applications are listed.

#### _lcrun /register_

Create a new application definition file. Here are the most common
options:

```bat
lcrun [-v] /register [-local] [-x <file.exe>] [-n <name>] [-F] ...
```

Options:
* -local : Create the appdef file in the current directory instead of
in the user store
* -x _\<file.exe>_ : the file to execute. Can be omitted if -base is
present
* -n _\<name>_ : overide the name for the appdef that was derived from
the -x option (if present). Required if there is no -x option.
* -F : enable overwriting an existing appdef
* -d _\<description>_  : set the description
* -base _\<apptag>_ : Create a derived application definition layered
on top of the specified existing base application definition. When
specified there should not be a -x option, since the executable is 
defined by the base already
* -a, -aa, -var : these options modify the argument list and
environment variables passed to the executable. See the output of
`"lcrun /r -h"` for more info. It may be more practical to edit the
generated application definition file instead of using these options.

It is expected that in future versions some common "recipes" for 
registering specific kinds of applications are added. Examples on my 
to-do list:
* Registering python virtual environments
* Registering python applications that use those virtual environments.
* Registering node.js applications

## Application Definition Files

Application definition files map a short "application name" to the
information needed to run an application. Application definition files
are JSON files that by convention use the double file extension
`*.appdef2.json`.

The current _lcrun_ implementation looks for application definition
files in two locations, called the _local store_ and the _user store_:

* The _local store_ is the current directory from which _lcrun_ was
  invoked.
* The _user store_ is a folder below the current user's home directory.
The exact location may be operating system dependent. 

Application definition files can contain the following information:

* The full path to the executable to run.
* A description
* A collection of environment variables to set, override or clear
* A collection of modifications to environment variables that represent
lists. This includes modifications to PATH.
* A list of arguments to insert before the arguments passed on the
call to lcrun.
* A list of arguments to append after those arguments
* A flag that determines if the directory of the executable should be
prepended to PATH. By default this is _true_, so this flag allows to 
prevent that behaviour.
* A working directory. Normally this is omitted and the original working
directory is preserved.
* A base "application name". If present, the application definition
extends the base application. 
    * In all reasonable cases, providing a base and providing an
      executable are mutually exclusive, since the base is already
      expected to define an executable.
    * The base appdef can itself also have a base, and so on.
    * To support this recursive definition model, the application
      definition model defines two passes, named "tobase" and
      "frombase" and most of the items above can occur in either
      (or both) passes.
    * Although there is no logical limit on the depth of recursion
      via bases, there is a practical limit of 10 levels to guard
      against accidental infinite recursion.

### Application context building process

The application definition file(s) are used to construct an 
_"application context"_ that gathers all information needed to run
the intended application. It contains the executable, working directory,
environment variables and arguments for the application. Understanding
how this context is built is easiest when looking at an application that
has a base application.

The basic principle is that _lcrun_ starts with a default environment,
which then goes through a series of modification steps before ending
up at the final version. Each application definition file contains
two such modification sections, one called _"tobase"_ and the other 
called _"frombase"_. The process works as follows:

* Start out with a default context, containing the environment variables
  passed to _lcrun_ itself, using the current working directory, 
  containing the arguments passed to _lcrun_, and not defining any 
  executable just yet.
* Apply the modifications from the _tobase_ section of the main
  application definition file.
* Apply the modifications from the _tobase_ section of the base
  application definition file.
* Repeat for all further bases, until you arrive at a base-less
  appdef.
* Now go through all application definition files again, but in 
  reverse order, applying the modifications from the _frombase_
  sections. So first apply the _frombase_ section of the final
  base application definition, then go back to the previous appdef,
  and so on, all the way until you are back at the initial application
  definition's _frombase_ section.

Now do a few steps and sanity checks:

* It is an error if at this point there is no executable set.
* Unless specified otherwise, prepend the directory of the excutable
  to the PATH.
* Construct the final environment (paying special attention to
  variables that were deleted)

... and then we have everything that is needed to actually run the
application.

### Application definition file structure

TBD

## Why "lcrun"?

Why did I name this tool "lcrun"? There is a bit of a history behind
that.

* In the past I already created a similar tool named "run.exe", and
  for the time being I wanted to have access to both tools. So I
  couldn't name it "run". It also turns out that simply "run" may
  cause conflicts with other existing tools.
* The original name for this new version was "runapp". It turned out
  that that name had some problems too. Most importantly: I kept
  having to check if it was "runapp" or "apprun". If I, as author,
  have to keep checking that, that is not a sign of a good name...
* In fact I noticed that even the code was referring to "runapp" at
  some times, and "apprun" at others.
* A bit of googling showed that some "logical" other candidates that
  were still somewhat snappy already had some other uses, sometimes
  even being associated with malware.
* Well, then go to the age old fallback solution to the problem of
  find good names: use your own initials in the name. My initials
  are LC, so that became "lcrun" ...
