// (c) 2022  ttelcl / ttelcl
module Usage

open CommonTools
open PrintUtils

let usage() =
  py "Application bootstrapper utility."
  pn "General synopsis is one of:"
  py "  runapp <shared-options> <apptag> <arguments>"
  py "  runapp <shared-options> /<command> <command-specific-options>"
  pn ""
  py "Shared options:"
  p2 "-v        Verbose mode"
  p2 "-sys      Only search appdefs in the system search folder"
  p2 "-usr      (default) Include user and system appdef search paths"
  p2 "-cwd      Also include the current directory in the appdef search path"
  pn ""
  py "runapp <options> [-dry] [-dmp] <apptag> <arguments>"
  pn "   Runs the application identified by <apptag> with the given arguments."
  p2 "-dry      Build the application execution information, but do not actually run it"
  p2 "-dmp      Dump the built application execution info to a file"
  pn ""
  py "runapp <options> /list [-q|-b|-v]"
  py "runapp <options> /l <arguments>"
  pn "  List the registered applications"
  p2 "-q        Quiet mode: only list names. Does not check for appdef JSON errors."
  p2 "-b        (default) Brief mode: list names, base and command"
  p2 "-v        Verbose mode: additionaly list description"
  pn ""
  py "runapp <options> /register <arguments>"
  py "runapp <options> /r <arguments>"
  pn "  Create a new (stub) appdef file. Consider further tuning in a text editor."
  p2 "-x <exe>          The (existing) executable file to register."
  p2 "-n <name>         The apptag (name) for the new appdef (default: derived from <exe>)"
  p2 "-base <tag>       The existing apptag for the base app (for layered appdefs)"
  p2 "-a <arg>          An argument to prepend (in the frombase phase). Repeatable."
  p2 "-var <var> <val>  An environment variable to set (frombase). Repeatable"
  p2 "-d <desc>         A description message. If omitted a stub will be generated"
  p2 "-F                Enable overwriting an existing appdef"

  

