module CmdRegister

open System
open System.IO
open System.Diagnostics
open System.Runtime.InteropServices;

open Newtonsoft.Json

open Lcl.RunLib.ApplicationDefinitions

open CommonTools
open ExceptionTool
open Usage
open FsRunUtils

type private RegisterOptions = {
  Shared: SharedArguments.SharedOptions
  Executable: string
  Name: string
  BaseTag: string
  Force: bool
}

let runRegister so args =
  let rec parsemore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parsemore o
    | "-x" :: exe :: rest
    | "-exe" :: exe :: rest
    | "-cmd" :: exe :: rest ->
      if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        if exe.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) |> not then
          failwith "The name of the executable command must end with '.exe' (on Windows)"
      rest |> parsemore {o with Executable = exe}
    | "-n" :: name :: rest
    | "-name" :: name :: rest ->
      rest |> parsemore {o with Name = name}
    | "-b" :: basetag :: rest
    | "-base" :: basetag :: rest ->
      rest |> parsemore {o with BaseTag = basetag}
    | "-F" :: rest
    | "-force" :: rest ->
      rest |> parsemore {o with Force = true}
    | [] ->
      if o.Executable = null && o.Name = null then
        failwith "At least one of '-n' or '-x' should be specified (to determine the appdef name)"
      o
    | x :: _ ->
      x |> failwithf "Unrecognized argument: '%s'"
  let o = args |> parsemore {
    Shared = so
    Executable = null
    Name = null
    BaseTag = null
    Force = false
  }
  let exe =
    if o.Executable |> String.IsNullOrEmpty then
      null
    elif o.Executable |> Path.IsPathFullyQualified then
      o.Executable
    else
      let attempt = o.Executable |> Path.GetFullPath
      if attempt |> File.Exists then
        attempt
      else
        failwith "Not yet implemented: resolving the executable against the PATH"
  if exe |> File.Exists |> not then
    exe |> failwithf "Target executable not found: %s"
  let name =
    if o.Name |> String.IsNullOrEmpty then
      exe |> Path.GetFileNameWithoutExtension
    else
      o.Name
  if name |> AppdefStore.IsValidApptag |> not then
    name |> failwithf "Not a valid apptag (use a -n option to override it): '%s'"
  let store = so.AppdefLocation |> AppdefStore.DefaultStore
  let filename = store.AppdefFileName(name)
  if filename |> File.Exists then
    if o.Force then
      color Color.DarkYellow
      printf "Warning! Overwriting existing "
      color Color.Yellow
      printf "%s" filename
      resetColor()
      printfn " ."
    else
      filename |> failwithf "Appdef already exists (use -F to overwrite): %s"
  else
    let upfile = name |> store.FindAppdefFile
    if upfile <> null then
      color Color.DarkYellow
      printf "Warning! The new appdef will shadow the existing appdef at "
      color Color.Yellow
      upfile |> printf "%s"
      resetColor()
      printfn " ."
  if (o.BaseTag |> String.IsNullOrEmpty) && (o.Executable |> String.IsNullOrEmpty) then
    // While this is likely an error, there are valid use cases imaginable where this happens.
    color Color.DarkYellow
    printf "Warning! Neither -x nor -base is specified. The resulting appdef will not be usable stand-alone"
    resetColor()
    printfn "."
  let description =
    if exe |> String.IsNullOrEmpty |> not then
      sprintf "(EDITME) Run %s" exe
    else
      sprintf "(EDITME) Application %s" name

  failwith "NotYetImplemented"
  0

