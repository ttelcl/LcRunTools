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
    cp ""
  
let usage_intro brief =
  cp "\fYApplication bootstrapper utility."
  cp "General synopsis is one of:"
  cp "  \fYrunapp\f0 <\fb-v\f0> <\fcapptag\f0> <\fgarguments\f0>"
  cp "  \fYrunapp\f0 <\fb-v\f0> \fY/\f0<\fycommand\f0> <\fgcommand-specific-options\f0>"
  cp ""
  brief |> usage_shared
  
let usage_run brief =
  cp "\fYrunapp\f0 [\fb-v\f0] [\fb-dry\f0] [\fb-dmp\f0] <\fcapptag\f0> <\fgarguments\f0>"
  cp "  Runs the application identified by <\fcapptag\f0> with the given \fgarguments\f0."
  if not brief then
    cp "\fb-dry\f0      Build the application execution information, but do not actually run it"
    cp "\fb-dmp\f0      Dump the built application execution info to a file"
    cp ""

let usage_list brief =
  cp "\fYrunapp\f0 [\fb-v\f0] \fy/list\f0 [\fg-q\f0|\fg-b\f0|\fg-v\f0] {\fg-m <txt>\f0}"
  if not brief then
    cp "  (alias: \fy/l\f0 instead of \fy/list\f0)"
  cp "  List the registered applications"
  if not brief then
    cp "\fg-h\f0        Print this /list help message and abort"
    cp "\fg-q\f0        Quiet mode: only list names. Does not check for appdef JSON errors."
    cp "\fg-b\f0        (default) Brief mode: list names, base and command"
    cp "\fg-v\f0        Verbose mode: additionaly list description"
    cp "\fg-m <txt>\f0  ('match') Only list applications whose names contain \fg<txt>\f0"
    cp "          Repeatable. If omitted: list all applications"
    cp ""
  
let usage_register brief =
  if not brief then
    cp "\fYrunapp\f0 [\fb-v\f0] \fy/register\f0 <\fgarguments\f0>"
    cp "  (alias: \fy/r\f0 instead of \fy/register\f0)"
  else
    cp "\fYrunapp\f0 [\fb-v\f0] \fy/register\f0 \fw...\f0"
    cp "  (use '\fYrunapp /help register\f0' or '\fYrunapp /register -h\f0' for full help)"
  cp "  Create a new (stub) appdef file. Consider further tuning in a text editor."
  if not brief then
    cp "\fg-h\f0 \fx\fx\fx\fx               Print this /register help message and abort"
    cp "\fg-local\f0 \fx\fx\fx\fx           Create in current directory instead of user store"
    cp "\fg-x\f0 <\fGexe\f0> \fx\fx         The (existing) executable file to register."
    cp "\fg-n\f0 <\fGname\f0> \fx\fx        The apptag (name) for the new appdef (default: derived from <\fGexe\f0>)"
    cp "\fg-base\f0 <\fGtag\f0> \fx\fx      The existing apptag for the base app (for layered appdefs)"
    cp "\fg-a\f0 <\fGarg\f0> \fx\fx         An argument to prepend (in the frombase phase). Repeatable."
    cp "\fg-aa\f0 <\fGarg\f0> \fx\fx        An argument to append (in the frombase phase). Repeatable."
    cp "\fg-var\f0 <\fGvar\f0> <\fGval\f0>  An environment variable to set (frombase). Repeatable"
    cp "\fg-d\f0 <\fGdesc\f0> \fx\fx        A description message. If omitted a stub will be generated"
    cp "\fg-F\f0 \fx\fx\fx\fx               Enable overwriting an existing appdef"
  
let usage_show brief =
  cp "\fYrunapp \fy/show\f0 <\fcapptag\f0>"
  cp "  Print the application definition (and base definitions)"

let usage_help brief =
  cp "\fYrunapp\f0 \fy/help\f0 [\fg/<command>\f0]"
  cp "  Print a more detailed version of this help message or the command's help message"
  if not brief then
    cp "\fg/<command>\f0        Only list help for the command (use '\fg/run\f0' for running help)"
    cp ""

let usage brief =
  brief |> usage_intro
  brief |> usage_run
  brief |> usage_help
  brief |> usage_list
  brief |> usage_show
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
  | Some("show") | Some("/show") ->
    usage_show false
  | None ->
    usage false
  | Some(x) ->
    x |> kprintf cp "\fyNo help available for \fr%s\f0. Showing full help instead."
    cp ""
    usage false
  
  
  
  
  

