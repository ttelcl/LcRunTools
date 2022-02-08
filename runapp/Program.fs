// (c) 2022  ttelcl / ttelcl

open System
open System.IO
open System.Text

open CommonTools
open ExceptionTool
open Usage
open SharedArguments

let run arglist =
  let so, cmd, restargs = arglist |> preparse
  match cmd with
  | ApprunCommand.List ->
    restargs |> CmdList.runCmdlist so
  | ApprunCommand.Run(target) ->
    restargs |> CmdRun.runRun so target
  | ApprunCommand.Register ->
    failwith "Not yet implemented: register"

[<EntryPoint>]
let main args =
  try
    // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-6.0#net-core-notes
    let provider = CodePagesEncodingProvider.Instance;
    Encoding.RegisterProvider(provider);
    
    match args.Length with
    | 0 ->
      usage()
      0
    | _ ->
      args |> Array.toList |> run
  with
    | ex ->
      ex |> fancyExceptionPrint verbose
      resetColor ()
      1



