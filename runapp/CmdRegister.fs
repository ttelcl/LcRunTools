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

type private VarAssign = {
  VarName: string
  VarValue: string
}

type private RegisterOptions = {
  Shared: SharedArguments.SharedOptions
  Executable: string
  Name: string
  BaseTag: string
  Force: bool
  Description: string
  VariablesFb: VarAssign list
  ArgumentsFb: string list
  ArgumentsAppendFb: string list
  AppdefLocation: StoreLocation
}

let runRegister so args =
  let rec parsemore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parsemore o
    | "-h" :: rest | "-help" :: rest | "/h" :: rest | "/help" :: rest ->
      usage_register false
      exit 0  // abort
    | "-usr" :: rest
    | "-user" :: rest ->
      rest |> parsemore {o with AppdefLocation = StoreLocation.User}
    | "-local" :: rest
    | "-cwd" :: rest ->
      rest |> parsemore {o with AppdefLocation = StoreLocation.Local}
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
    | "-d" :: desc :: rest ->
      rest |> parsemore {o with Description = desc}
    | "-a" :: arg :: rest 
    | "-ap" :: arg :: rest ->
      rest |> parsemore {o with ArgumentsFb = arg :: o.ArgumentsFb}
    | "-aa" :: arg :: rest ->
      rest |> parsemore {o with ArgumentsAppendFb = arg :: o.ArgumentsAppendFb}
    | "-var" :: varname :: varvalue :: rest 
    | "-set" :: varname :: varvalue :: rest ->
      let v = {
        VarName = varname
        VarValue = varvalue
      }
      rest |> parsemore {o with VariablesFb = v :: o.VariablesFb}
    | [] ->
      if o.Executable = null && o.Name = null then
        usage_register false
        failwith "At least one of '-n' or '-x' should be specified (to determine the appdef name)"
      {o with
        ArgumentsFb = o.ArgumentsFb |> List.rev
        ArgumentsAppendFb = o.ArgumentsAppendFb |> List.rev
        VariablesFb = o.VariablesFb |> List.rev}
    | x :: _ ->
      x |> failwithf "Unrecognized argument: '%s'"
  let o = args |> parsemore {
    Shared = so
    Executable = null
    Name = null
    BaseTag = null
    Force = false
    Description = null
    VariablesFb = []
    ArgumentsFb = []
    ArgumentsAppendFb = []
    AppdefLocation = StoreLocation.User
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
  if exe <> null && exe |> File.Exists |> not then
    exe |> failwithf "Target executable not found: %s"
  let name =
    if o.Name |> String.IsNullOrEmpty then
      exe |> Path.GetFileNameWithoutExtension
    else
      o.Name
  if name |> AppdefStore.IsValidApptag |> not then
    name |> failwithf "Not a valid apptag (use a -n option to override it): '%s'"
  // let store = so.AppdefLocation |> AppdefStore.DefaultStore
  let store =
    match o.AppdefLocation with
    | StoreLocation.Local -> AppdefStore.DefaultLocalStore
    | StoreLocation.User -> AppdefStore.DefaultUserStore
    | StoreLocation.System ->
      failwithf "System store support has been disabled"
    | x ->
      failwithf "Unrecognized store location code %O" x
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
    if o.Description |> String.IsNullOrEmpty |> not then
      o.Description
    elif exe |> String.IsNullOrEmpty |> not then
      sprintf "(EDIT ME) Run %s" exe
    else
      sprintf "(EDIT ME) Application %s" name
  if o.BaseTag |> String.IsNullOrEmpty |> not then
    if o.BaseTag |> AppdefStore.IsValidApptag |> not then
      failwithf "The given base tag is not a valid apptag: %s" o.BaseTag
    let foundBase = store.FindAppdefFile(o.BaseTag, true)
    if foundBase = null then
      failwithf "Unknown base tag: %s" o.BaseTag
  let vars = new System.Collections.Generic.Dictionary<string, string>()
  for vdef in o.VariablesFb do
    vars.[vdef.VarName] <- vdef.VarValue
  let arglist =
    new ListMutation(prepend = o.ArgumentsFb, append = o.ArgumentsAppendFb)
  arglist.SerializeVerbose <- true
  let tobase =
    new InvocationMutationPhase() // empty!
  let frombase =
    new InvocationMutationPhase(
      command = exe,
      prependCommandPath = new Nullable<bool>(),
      workdir = null,
      vars = vars,
      lists = null,
      args = arglist
      )
  frombase.SerializeFullVarsVerbose <- true
  let im =
    new InvocationMutation(o.BaseTag, tobase, frombase, description)
  let json = JsonConvert.SerializeObject(im, Formatting.Indented)

  color Color.Red
  printf "DEBUG"
  resetColor()
  printfn ":"
  printfn "%s" json

  if store.Exists |> not then
    printf "Creating store folder: "
    color Color.Yellow
    store.Folder |> printf "%s"
    resetColor()
    printfn "."
    store.Folder |> Directory.CreateDirectory |> ignore
  do
    use txt = filename |> startFile
    txt.WriteLine(json)
  filename |> finishFile

  0

