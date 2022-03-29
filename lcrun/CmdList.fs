module CmdList

open System
open System.IO

open Lcl.RunLib.ApplicationDefinitions

open CommonTools
open ExceptionTool
open Usage
open FsRunUtils
open ColorPrint

type private ListVerbosity =
  | Quiet = 0
  | Normal = 1
  | Verbose = 2

type private ListOptions = {
  Shared: SharedArguments.SharedOptions
  Verbosity: ListVerbosity
  Filters: string list
}

let runCmdlist (so: SharedArguments.SharedOptions) args =
  let rec parseMore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parseMore {o with Verbosity = ListVerbosity.Verbose}
    | "-h" :: rest | "-help" :: rest | "/h" :: rest | "/help" :: rest ->
      usage_list false
      exit 0  // abort
    | "-q" :: rest ->
      rest |> parseMore {o with Verbosity = ListVerbosity.Quiet}
    | "-b" :: rest ->
      rest |> parseMore {o with Verbosity = ListVerbosity.Normal}
    | "-m" :: filter :: rest ->
      rest |> parseMore {o with Filters = filter :: o.Filters}
    | x :: _ ->
      x |> failwithf "Unrecognized option for /list: %s"
    | [] ->
      {o with Filters = o.Filters |> List.rev}
  let lo =
    args |> parseMore {
      Shared = so
      Verbosity = if verbose then ListVerbosity.Verbose else ListVerbosity.Normal
      Filters = []
    }
  let stores = AppdefStore.DefaultLocalStore |> storeList
  let filterTag = // filter according to "-m" options
    if lo.Filters |> List.isEmpty then
      fun (tag:string) -> Some(tag)
    else
      fun (tag: string) ->
        if lo.Filters |> List.exists (fun filter -> tag.Contains(filter)) then
          Some(tag)
        else
          None
  if lo.Filters |> List.isEmpty |> not then
    cpx "Listing applications matching any of:"
    for i, f in lo.Filters |> Seq.indexed do
      if i > 0 then
        f |> sprintf " / \fg%s\f0" |> cpx
      else
        f |> sprintf " \fg%s\f0" |> cpx
    cp ""
  for store in stores do
    let tags =
      if store.Exists then
        store.ListAppTags() |> Seq.sort |> Seq.choose filterTag |> Seq.toArray
      else
        [||]
    if not(store.Quiet) || tags.Length > 0 then // else: stay quiet
      store.Label |> sprintf "In store \fc%s\f0 " |> cpx
      if not store.Exists then
        store.Folder |> sprintf "(\fR[missing]\fk%s\f0)" |> cp
      else
        store.Folder |> sprintf "(\fb%s\f0):" |> cp
        if tags.Length > 0 then
          for tag in tags do
            tag |> sprintf "  \fg%-20s\f0" |> cpx
            if lo.Verbosity <> ListVerbosity.Quiet then
              try
                let im = tag |> store.LoadAppdefBare
                if im.BaseName <> null then
                  im.BaseName |> sprintf " \fY<- %s\f0" |> cpx
                if im.Command |> String.IsNullOrEmpty |> not then
                  im.Command |> sprintf " \fk%s\f0" |> cpx
                if lo.Verbosity = ListVerbosity.Verbose then
                  if im.Description <> null then
                    printfn ""
                    printf "        %s" im.Description
              with
                | ex ->
                  ex.GetType().Name |> sprintf " \fr%s\f0:" |> cpx
                  ex.Message |> sprintf " \fY%s\f0" |> cpx
            printfn ""
        else
          "  \fk(no application definitions found)\f0" |> cp
  0



