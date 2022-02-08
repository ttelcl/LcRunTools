module CommonTools

open System
open System.IO
open System.Threading

type Color = ConsoleColor
let color clr =
  Console.ForegroundColor <- clr
let bcolor clr =
  Console.BackgroundColor <- clr
let color2 fclr bclr =
  Console.ForegroundColor <- fclr
  Console.BackgroundColor <- bclr

let resetColor () =
  Console.ResetColor()

// Note the modification compared to the usual version: background color
// and use of stderr instead of stdout
let startFile name =
  let tmp = name + ".tmp"
  bcolor Color.DarkBlue
  color Color.Gray // expected to be the normal text color, but be explicit just to be sure
  eprintf "Writing '"
  color Color.DarkYellow
  eprintf "%s" name
  color Color.Gray
  eprintf "'"
  resetColor()
  eprintfn ""
  File.CreateText(tmp)

let finishFile name =
  let tmp = name + ".tmp"
  if File.Exists(name) then
    let bak = name + ".bak"
    File.Replace(tmp, name, bak)
  else
    File.Move(tmp, name)

let mutable verbose = false

/// Generic argument list splitter: turns a list of strings
/// into a list of lists of strings where the first string starts with a '-'
let splitArgs args =
  let rec cleanAndReverse lo l =
    match l with
    | [] :: rest ->
      rest |> cleanAndReverse lo
    | x :: rest ->
      rest |> cleanAndReverse (x :: lo)
    | [] ->
      lo
  let rec splitInner ll l (rest:string list) =
    match rest with
    | o :: tail when o.StartsWith("-") ->
      let lr = l |> List.rev
      tail |> splitInner (lr :: ll) [o]
    | a :: tail ->
      tail |> splitInner ll (a :: l)
    | [] ->
      let lr = l |> List.rev
      let lx = lr :: ll
      lx |> cleanAndReverse []
  args |> splitInner [] []

/// Split a list of strings into two lists, the first of which
/// will have no items starting with "-" except possibly the first
/// element
let splitNoDash args =
  let rec splitMore l1 (l2: string list) =
    match l2 with
    | x :: rest when not(x.StartsWith("-")) ->
      splitMore (x :: l1) rest
    | [] ->
      (l1 |> List.rev), l2
    | rest ->
      (l1 |> List.rev), rest
  match args with
  | x :: rest ->
    splitMore [x] rest
  | [] ->
    [], []

let consoleCancelToken =
  let cts = new CancellationTokenSource()
  let token = cts.Token
  Console.CancelKeyPress.Add(
      fun args ->
        if token.IsCancellationRequested then
          color ConsoleColor.Red
          printfn "Second CTRL-C: aborting"
          resetColor ()
        else
          color ConsoleColor.Yellow
          printfn "Stopping ...!"
          resetColor ()
          cts.Cancel()
          args.Cancel <- true
        )
  token

let canceled =
  fun () -> consoleCancelToken.IsCancellationRequested

