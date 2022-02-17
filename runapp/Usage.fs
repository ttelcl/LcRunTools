// (c) 2022  ttelcl / ttelcl
module Usage

open CommonTools
open PrintUtils
open ColorPrint
open Printf

let usage_shared brief =
  if not brief then
    cp "\fbShared options\f0:"
    cp "\fb-v\f0        Verbose mode"
    cp "\fb-sys\f0      Only search appdefs in the system search folder"
    cp "\fb-usr\f0      (default) Include user and system appdef search paths"
    cp "\fb-cwd\f0      Also include the current directory in the appdef search path"
    cp ""
  
let usage_intro brief =
  cp "\fYApplication bootstrapper utility."
  cp "General synopsis is one of:"
  cp "  \fYrunapp\f0 <\fbshared-options\f0> <\fcapptag\f0> <\fgarguments\f0>"
  cp "  \fYrunapp\f0 <\fbshared-options\f0> \fY/\f0<\fycommand\f0> <\fgcommand-specific-options\f0>"
  cp ""
  brief |> usage_shared
  
let usage_run brief =
  cpx "\fYrunapp\f0 [\fb-v\f0] [\fb-sys\f0|\fb-usr\f0|\fb-cwd\f0] [\fb-dry\f0] [\fb-dmp\f0]"
  cp " <\fcapptag\f0> <\fgarguments\f0>"
  cp "  Runs the application identified by <\fcapptag\f0> with the given \fgarguments\f0."
  if not brief then
    cp "\fb-dry\f0      Build the application execution information, but do not actually run it"
    cp "\fb-dmp\f0      Dump the built application execution info to a file"
    cp ""

let usage_list brief =
  cp "\fYrunapp\f0 [\fb-v\f0] [\fb-sys\f0|\fb-usr\f0|\fb-cwd\f0] \fy/list\f0 [\fg-q\f0|\fg-b\f0|\fg-v\f0]"
  if not brief then
    cp "  (alias: \fy/l\f0 instead of \fy/list\f0)"
  cp "  List the registered applications"
  if not brief then
    cp "\fg-q\f0        Quiet mode: only list names. Does not check for appdef JSON errors."
    cp "\fg-b\f0        (default) Brief mode: list names, base and command"
    cp "\fg-v\f0        Verbose mode: additionaly list description"
    cp ""
  
let usage_register brief =
  cp "\fYrunapp\f0 [\fb-v\f0] [\fb-sys\f0|\fb-usr\f0|\fb-cwd\f0] /register <\fgarguments\f0>"
  if not brief then
    cp "  (alias: \fy/r\f0 instead of \fy/register\f0)"
  cp "  Create a new (stub) appdef file. Consider further tuning in a text editor."
  if not brief then
    cp "\fg-x\f0 <\fGexe\f0> \fx\fx         The (existing) executable file to register."
    cp "\fg-n\f0 <\fGname\f0> \fx\fx        The apptag (name) for the new appdef (default: derived from <\fGexe\f0>)"
    cp "\fg-base\f0 <\fGtag\f0> \fx\fx      The existing apptag for the base app (for layered appdefs)"
    cp "\fg-a\f0 <\fGarg\f0> \fx\fx         An argument to prepend (in the frombase phase). Repeatable."
    cp "\fg-var\f0 <\fGvar\f0> <\fGval\f0>  An environment variable to set (frombase). Repeatable"
    cp "\fg-d\f0 <\fGdesc\f0> \fx\fx        A description message. If omitted a stub will be generated"
    cp "\fg-F\f0 \fx\fx\fx\fx               Enable overwriting an existing appdef"

let usage_help brief =
  cp "\fYrunapp\f0 \fy/help\f0 [\fg/<command>\f0]"
  cp "  Print a more detailed version of this help message"
  if not brief then
    cp "\fg/<command>\f0        Only list help for the command (use '/run' for apptag mode)"
    cp ""

let usage brief =
  brief |> usage_intro
  brief |> usage_run
  brief |> usage_help
  brief |> usage_list
  brief |> usage_register
  
let runHelp arg =
  match arg with
  | Some("list") | Some("l") | Some("/list") | Some("/l") ->
    usage_list false
    usage_shared false
  | Some("register") | Some("r") | Some("/register") | Some("/r") ->
    usage_register false
    usage_shared false
  | Some("run") | Some("apptag") | Some("/run") | Some("/apptag") ->
    usage_run false
    usage_shared false
  | Some("help") | Some("h") | Some("/help") | Some("/h") | Some("-help") | Some("-h") ->
    usage_help false
  | None ->
    usage false
  | Some(x) ->
    x |> kprintf cp "\fyNo help available for \fr%s\f0. Showing full help instead."
    cp ""
    usage false
  
  
  
  
  

