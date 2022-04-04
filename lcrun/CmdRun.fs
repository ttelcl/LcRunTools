module CmdRun

open System
open System.IO
open System.Diagnostics

open Newtonsoft.Json

open Lcl.RunLib.ApplicationDefinitions

open CommonTools
open ColorPrint
open ExceptionTool
open Usage
open FsRunUtils


let runRun (so:SharedArguments.SharedOptions) target appargs =
  let store = AppdefStore.DefaultLocalStore
  let mutation = target |> store.LoadAppdef
  if verbose then
    // Reminder: use eprintf and eprintfn (-> ColorPrint.ecp) instead of
    // printf and printfn (ColorPrint.cp)
    // so the behaviour is correct when redirecting the output

    //bcolor Color.DarkBlue
    //eprintf "Loaded application definition chain:"
    //resetColor()
    //eprintfn " "
    ecp "\vBLoaded application definition chain:\v0 "
    for node in mutation |> mutationNodeList do
      //bcolor Color.DarkBlue
      //color Color.Yellow
      //eprintf "  %-20s" node.Tag
      //color Color.Green
      //eprintf " %s" node.FileName
      //resetColor()
      //eprintfn " "
      sprintf "\vB\fy  %-20s\fg %s\f0 " node.Tag node.FileName |> ecp
  let invocation = appargs |> InvocationModel.CreateDefault
  mutation.ApplyTo(invocation)
  invocation.Finish()
  if verbose then
    //bcolor Color.DarkBlue
    //eprintf "Command and arguments:"
    //resetColor()
    //eprintfn " "
    ecp "\vBCommand and arguments:\v0 "
    
    //eprintf "  "
    //bcolor Color.DarkBlue
    //color Color.Yellow
    //eprintf "%s" invocation.Executable
    //resetColor()
    //eprintfn " "
    sprintf "  \vB\fy%s\f0 " invocation.Executable |> ecp
    for arg in invocation.Arguments do
      //eprintf "  "
      //bcolor Color.DarkBlue
      //eprintf "%s" arg
      //resetColor()
      //eprintfn " "
      sprintf "  \vB%s\v0 " arg |> ecp
    //bcolor Color.DarkBlue
    //eprintf "Start folder:"
    //resetColor()
    //eprintfn " "
    ecp "\vBStart folder:\v0 "
    //eprintf "  "
    //bcolor Color.DarkBlue
    //color Color.DarkYellow
    //eprintf "%s" invocation.WorkingDirectory
    //resetColor()
    //eprintfn " "
    sprintf "  \vB\fo%s\f0 " invocation.WorkingDirectory |> ecp
  if so.DoDump then
    let json = JsonConvert.SerializeObject(invocation, Formatting.Indented)
    let onm = sprintf "%s.run-invocation-dump.json" target
    do
      use w = onm |> startFile
      w.WriteLine(json)
    onm |> finishFile
  if so.DryRun then
    //color Color.Red
    //eprintf "[[Dry-run: not actually executing the command]]"
    //resetColor()
    //eprintfn " "
    ecp "\fr[[Dry-run: not actually executing the command]]\f0 "
    0
  else
    let psi = invocation.Build()
    if verbose then
      //bcolor Color.DarkBlue
      //eprintf "---- lcrun: Starting process ----"
      //resetColor()
      //eprintfn " "
      ecp "\vB---- lcrun: Starting process ----\v0 "
    let exitCode =
      use prc = Process.Start(psi)
      prc.WaitForExit(-1) |> ignore
      prc.ExitCode
    if verbose then
      //bcolor Color.DarkBlue
      //color Color.Gray
      //eprintf "---- lcrun: Process finished. Status code = "
      //if exitCode = 0 then
      //  color Color.Green
      //else
      //  color Color.Red
      //eprintf "%d" exitCode
      //color Color.Gray
      //eprintf " ----"
      //resetColor()
      //eprintfn " "
      sprintf "\vB\fa---- lcrun: Process finished. Status code = " |> ecpx
      (if exitCode = 0 then "\fg" else "\fr") |> ecpx
      sprintf "%d\fa ----\v0 " exitCode |> ecp
    exitCode  
