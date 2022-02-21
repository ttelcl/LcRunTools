﻿module SharedArguments

// parses arguments shared by the modes

open System
open System.IO

open Lcl.RunLib.ApplicationDefinitions

open CommonTools
open ExceptionTool
open Usage
open ColorPrint

type ApprunCommand =
  | Run of string
  | List
  | Register
  | Help of string option
  | Show

type SharedOptions = {
  DryRun: bool
  DoDump: bool
  // AppdefLocation: StoreLocation
}

/// Parse common arguments up to and including the command
/// Returns a triplet: the shared options, the command, the remaining arguments
let preparse args =
  let rec preparsemore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> preparsemore o
    | "-dry" :: rest ->
      rest |> preparsemore {o with DryRun = true}
    | "-dmp" :: rest ->
      rest |> preparsemore {o with DoDump = true}
    | [] ->
      failwith "No command given (an apptag, or -l or -r option)"
    | "/l" :: rest
    | "/list" :: rest ->
      o, ApprunCommand.List, rest
    | "/r" :: rest
    | "/register" :: rest ->
      o, ApprunCommand.Register, rest
    | "/h" :: arg :: rest
    | "-h" :: arg :: rest
    | "-help" :: arg :: rest
    | "/help" :: arg :: rest ->
      o, ApprunCommand.Help(Some(arg)), rest
    | "/h" :: rest
    | "-h" :: rest
    | "-help" :: rest
    | "/help" :: rest ->
      o, ApprunCommand.Help(None), rest
    | "/show" :: rest ->
      o, ApprunCommand.Show, rest
    | "/show" :: [] ->
      ecp "\fY'/show' expects an apptag as argument\f0. Running \fY/list\f0 instead"
      o, ApprunCommand.List, []
    | apptag :: rest when not(apptag.StartsWith('-') || apptag.StartsWith('/')) ->
      o, ApprunCommand.Run(apptag), rest
    | x :: _ when x.StartsWith('/') ->
      failwithf "Unrecognized command: %s" x
    | x :: _ ->
      failwithf "Unrecognized pre-command option: %s" x
  args |> preparsemore {
    DryRun = false
    // AppdefLocation = StoreLocation.User
    DoDump = false
  }

