module ExceptionTool

open System
open System.Diagnostics
open System.IO

open CommonTools

let rec fancyExceptionPrint showTrace (ex:Exception) =
  try
    color Color.Red
    eprintf "%s" (ex.GetType().FullName)
    resetColor()
    eprintf ": "
    color Color.DarkYellow
    eprintf "%s" ex.Message
    resetColor()
    eprintfn ""
    if showTrace then
      let trace = new StackTrace(ex, true)
      for frame in trace.GetFrames() do
        eprintf "  "
        let fnm =
          if frame.HasSource() then
            let fnm = frame.GetFileName()
            color Color.Red
            eprintf "%15s" (Path.GetFileName(fnm))
            resetColor()
            eprintf ":"
            color Color.Green
            eprintf "%4d" (frame.GetFileLineNumber())
            resetColor()
            fnm
          else
            color Color.Red
            eprintf "%15s" "?"
            resetColor()
            eprintf ":"
            eprintf "    "
            resetColor()
            null
        if frame.HasMethod() then
          let method = frame.GetMethod()
          eprintf " "
          color Color.Yellow
          eprintf "%s" (method.Name)
          eprintf "("
          let pinfs = method.GetParameters()
          if pinfs.Length>0 then
            color Color.DarkYellow
            eprintf "[%d]" pinfs.Length
            color Color.Yellow
            eprintf ")"
          else
            eprintf ")"
          color Color.DarkGray
          eprintf " "
          color Color.White
          eprintf "%s" (method.ReflectedType.Name)
          resetColor()
        else
          color Color.Red
          eprintf "(?)"
          resetColor()
        if fnm <> null then
          color Color.DarkGray
          eprintf " (%s)" (Path.GetDirectoryName(fnm))
          resetColor()
        eprintfn ""
      ()
    finally
      resetColor()
  if ex.InnerException <> null then
    color Color.Cyan
    eprintf "----> "
    resetColor()
    ex.InnerException |> fancyExceptionPrint showTrace


