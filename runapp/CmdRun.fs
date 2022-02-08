module CmdRun

open System
open System.IO
open System.Diagnostics

open Newtonsoft.Json

open Lcl.RunLib.ApplicationDefinitions

open CommonTools
open ExceptionTool
open Usage
open FsRunUtils


let runRun (so:SharedArguments.SharedOptions) target appargs =
  let store = so.AppdefLocation |> AppdefStore.DefaultStore
  let mutation = target |> store.LoadAppdef
  if verbose then
    // Reminder: use eprintf and eprintfn instead of printf and printfn
    // so the behaviour is correct when redirecting the output
    bcolor Color.DarkBlue
    eprintf "Loaded application definition chain:"
    resetColor()
    eprintfn " "
    for node in mutation |> mutationNodeList do
      bcolor Color.DarkBlue
      color Color.Yellow
      eprintf "  %-20s" node.Tag
      color Color.Green
      eprintf " %s" node.FileName
      resetColor()
      eprintfn " "
  let invocation = appargs |> InvocationModel.CreateDefault
  mutation.ApplyTo(invocation)
  invocation.Finish()
  if verbose then
    bcolor Color.DarkBlue
    eprintf "Command and arguments:"
    resetColor()
    eprintfn " "
    eprintf "  "
    bcolor Color.DarkBlue
    color Color.Yellow
    eprintf "%s" invocation.Executable
    resetColor()
    eprintfn " "
    for arg in invocation.Arguments do
      eprintf "  "
      bcolor Color.DarkBlue
      eprintf "%s" arg
      resetColor()
      eprintfn " "
    bcolor Color.DarkBlue
    eprintf "Start folder:"
    resetColor()
    eprintfn " "
    eprintf "  "
    bcolor Color.DarkBlue
    color Color.DarkYellow
    eprintf "%s" invocation.WorkingDirectory
    resetColor()
    eprintfn " "
  if so.DoDump then
    let json = JsonConvert.SerializeObject(invocation, Formatting.Indented)
    let onm = sprintf "%s.run-invocation-dump.json" target
    do
      use w = onm |> startFile
      w.WriteLine(json)
    onm |> finishFile
    ()
  if so.DryRun then
    color Color.Red
    eprintf "[[Dry-run: not actually executing the command]]"
    resetColor()
    eprintfn " "
    0
  else
    let psi = invocation.Build()
    if verbose then
      bcolor Color.DarkBlue
      eprintf "---- apprun: Starting process ----"
      resetColor()
      eprintfn " "
    let exitCode =
      use prc = Process.Start(psi)
      prc.WaitForExit(-1) |> ignore
      prc.ExitCode
    if verbose then
      bcolor Color.DarkBlue
      color Color.Gray
      eprintf "---- apprun: Process finished. Status code = "
      if exitCode = 0 then
        color Color.Green
      else
        color Color.Red
      eprintf "%d" exitCode
      color Color.Gray
      eprintf " ----"
      resetColor()
      eprintfn " "
    exitCode  
