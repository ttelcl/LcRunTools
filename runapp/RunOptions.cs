/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lcl.RunApp
{
  /// <summary>
  /// A place to store execution modifiers for the runapp driver:
  /// an interpreted version of the arguments
  /// </summary>
  public class RunOptions
  {
    private bool _initialized = false;

    /// <summary>
    /// Create a new empty RunOptions
    /// </summary>
    public RunOptions()
    {
      Action = "run";
      TargetArgs = new List<string>();
    }

    public void Initialize(IEnumerable<string> args)
    {
      if(_initialized)
      {
        throw new InvalidOperationException(
          "Double RunOptions initialization");
      }
      _initialized = true;
      var runargs = args.ToList();
      while(runargs.Count > 0 && runargs[0].StartsWith('-'))
      {
        var flag = runargs[0];
        runargs.RemoveAt(0);
        switch(flag)
        {
          case "-v":
            Verbose = true;
            break;
          case "-dry":
            RunDry = true;
            break;
          case "-l":
          case "-list":
            Action = "list";
            break;
          case "-r":
          case "-register":
            Action = "register";
            break;
          case "-h":
          case "-help":
            Action = "help";
            break;
          default:
            throw new InvalidOperationException(
              $"Unrecognized flag or command '{flag}'");
        }
      }
      if(runargs.Count > 0)
      {
        TargetName = runargs[0];
        runargs.RemoveAt(0);
      }
      TargetArgs.AddRange(runargs);
      if(String.IsNullOrEmpty(TargetName) && Action == "run")
      {
        Action = "help";
      }
    }

    /// <summary>
    /// The action to take (normally "run" if not specified otherwise)
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// The target of the action. Normally the name of the app to run (if the
    /// action is "run"). May be null in some situations.
    /// </summary>
    public string? TargetName { get; set; }

    /// <summary>
    /// The arguments to pass to the child process (in some cases the remaining args
    /// to be handled by a subcommand instead)
    /// </summary>
    public List<string> TargetArgs { get; }

    /// <summary>
    /// Affects verbosity of exception traces of runapp.exe itself, and of other
    /// messages
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    /// When true
    /// </summary>
    public bool RunDry { get; set; }
  }
}
