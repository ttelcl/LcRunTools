module CmdList

open System
open System.IO

open Lcl.RunLib.ApplicationDefinitions

open CommonTools
open ExceptionTool
open Usage
open FsRunUtils

type private ListVerbosity =
  | Quiet = 0
  | Normal = 1
  | Verbose = 2

type private ListOptions = {
  Shared: SharedArguments.SharedOptions
  Verbosity: ListVerbosity
}

let runCmdlist (so: SharedArguments.SharedOptions) args =
  let rec parseMore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parseMore {o with Verbosity = ListVerbosity.Verbose}
    | "-q" :: rest ->
      rest |> parseMore {o with Verbosity = ListVerbosity.Quiet}
    | "-b" :: rest ->
      rest |> parseMore {o with Verbosity = ListVerbosity.Normal}
    | x :: _ ->
      x |> failwithf "Unrecognized option for /list: %s"
    | [] ->
      o
  let lo =
    args |> parseMore {
      Shared = so
      Verbosity = if verbose then ListVerbosity.Verbose else ListVerbosity.Normal
    }
  // let stores = so.AppdefLocation |> AppdefStore.DefaultStore |> storeList
  let stores = AppdefStore.DefaultLocalStore |> storeList
  for store in stores do
    let tags =
      if store.Exists then
        store.ListAppTags() |> Seq.sort |> Seq.toArray
      else
        [||]
    if not(store.Quiet) || tags.Length > 0 then // else: stay quiet
      printf "In store "
      color Color.Cyan
      printf "%-8s" store.Label
      resetColor()
      printf " ("
      if not store.Exists then
        color Color.DarkRed
        printf "[missing] "
        color Color.DarkGray
        printf "%s" store.Folder
        resetColor()
        printfn ")"
      else
        color Color.Blue
        printf "%s" store.Folder
        resetColor()
        printfn "):"
        if tags.Length > 0 then
          for tag in tags do
            color Color.Green
            printf "  %-20s" tag
            resetColor()
            if lo.Verbosity <> ListVerbosity.Quiet then
              try
                let im = tag |> store.LoadAppdefBare
                if im.BaseName <> null then
                  printf " "
                  color Color.DarkYellow
                  printf "<- %s" im.BaseName
                  resetColor()
                  printf ""
                if im.Command |> String.IsNullOrEmpty |> not then
                  printf " "
                  color Color.DarkGray
                  printf "%s" im.Command
                  resetColor()
                if lo.Verbosity = ListVerbosity.Verbose then
                  if im.Description <> null then
                    printfn ""
                    printf "        %s" im.Description
              with
                | ex ->
                  color Color.Red
                  ex.GetType().Name |> printf " %s:"
                  ex.Message |> printf "%s"
                  resetColor()
            printfn ""
        else
          color Color.DarkGray
          printf "  (no application definitions found)"
          resetColor()
          printfn ""
  0



