module CmdShow

open System
open System.IO

open Newtonsoft.Json

open Lcl.RunLib.ApplicationDefinitions

open CommonTools
open ExceptionTool
open Usage
open ColorPrint

type private ShowOptions = {
  Shared: SharedArguments.SharedOptions
  AppTag: string
}

let rec private printAppdef (store: AppdefStore) appdeffile =
  if appdeffile |> File.Exists |> not then
    appdeffile |> failwithf "Application definition file not found: %s"
  let json = appdeffile |> File.ReadAllText
  appdeffile |> sprintf "\fb%s\f0:" |> cp
  json |> printfn "%s"
  let im = JsonConvert.DeserializeObject<InvocationMutation>(json)
  if im.BaseName |> String.IsNullOrEmpty then
    0
  else
    let ok, basedeffile, defstore = store.TryFindAppdefFile(im.BaseName, true)
    if ok |> not then
      im.BaseName |> sprintf "\frUnknown base application definition\f0: \fY%s\f0" |> cp
      1
    else
      basedeffile |> printAppdef defstore

let runShow so args =
  let rec parsemore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parsemore o
    | apptag :: rest when (apptag.StartsWith("-") |> not) ->
      rest |> parsemore {o with AppTag = apptag}
    | [] ->
      if o.AppTag |> String.IsNullOrEmpty then
        failwith "No application tag to show specified"
      o
    | x :: _ ->
      x |> failwithf "Unrecognized argument: '%s'"
  let o = args |> parsemore {
    Shared = so
    AppTag = null
  }
  let store = AppdefStore.DefaultLocalStore
  let ok, appdeffile, defstore = store.TryFindAppdefFile(o.AppTag, true)
  if ok |> not then
    o.AppTag |> sprintf "\frUnknown application definition\f0: \fY%s\f0" |> cp
    1
  else
    appdeffile |> printAppdef defstore


