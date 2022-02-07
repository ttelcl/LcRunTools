// (c) 2022  ttelcl / ttelcl
module Usage

open CommonTools
open PrintUtils

let usage() =
  py "Application bootstrapper utility."
  pn ""
  py "runapp [-v] [-dry] [-cwd|-usr|-sys] <apptag> <arguments>"
  pn "   Runs the application identified by <apptag> with the given arguments."
  p2 "-v           Verbose mode"
  p2 "-dry         Build the application execution information, but do not actually run it"
  p2 "-cwd         Include the current directory in the appdef search path (+user, +system)"
  p2 "-usr         (default) Include user and system appdef search paths"
  p2 "-sys         Only search appdefs in the system search folder"
  pn ""
  py "runapp -l"
  py "runapp -list"
  pn "  List the registered applications"
  pn ""
  py "runapp -r <arguments>"
  pn "  Create a new appdef file (syntax TBD)"


