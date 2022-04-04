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
_application definition file_ for "myapp", and run your app as

```bat
lcrun myapp arg1 arg2 arg3
```

Using your application definition file, lcrun can find where
myapp.exe actually is located, can tweak the run environment
(by default: put the application's directory at the start of PATH)
and the execute myapp.exe and pass the arguments to it.

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

## Application Definition Files

TBD

## Usage

TBD



