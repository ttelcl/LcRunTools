// (c) 2022  ttelcl / ttelcl

open System
open System.IO

open CommonTools
open ExceptionTool
open Usage
open SharedArguments

let run arglist =
  let so, cmd, restargs = arglist |> preparse
  match cmd with
  | ApprunCommand.List ->
    failwith "Not yet implemented: list"
  | ApprunCommand.Run(target) ->
    failwithf "Not yet implemented: run (%s)" target
  | ApprunCommand.Register ->
    failwith "Not yet implemented: register"

[<EntryPoint>]
let main args =
  try
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



