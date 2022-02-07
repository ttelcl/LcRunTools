using System;

namespace Lcl.RunApp // Note: actual namespace depends on the project name.
{
  internal class Program
  {
    static RunOptions _options = new RunOptions();

    static void Yellow(string text)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(text);
      Console.ResetColor();
    }

    static void Gray(string text)
    {
      Console.WriteLine(text);
    }

    static void GreenGray(string text1, string text2)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.Write("{0,-12}", text1);
      Console.ResetColor();
      Console.WriteLine(text2);
    }

    static int ShowHelp(RunOptions o)
    {
      Yellow(   "apprun [-v] [-dry] [-cwd|-sys|-usr] <apptag> {arguments}");
      Gray(     "  Runs the application identified by <apptag> with the given arguments");
      GreenGray("  -v", "Verbose mode");
      GreenGray("  -dry", "(implies -v). Prepare but do not actually run");
      GreenGray("  -cwd", "Also look in current directory for appdefs (for testing & debugging)");
      GreenGray("  -sys", "Only look in the system-wide appdef directory");
      GreenGray("  -usr", "(default) Look in the user and system-wide appdef directories");

      Gray("");
      Yellow(   "apprun [-v] [-l|-list]");
      Gray(     "  List registered applications");

      Gray("");
      Yellow(   "apprun [-r|-register] [-x <executable.exe>] [-F]");
      Gray(     "  Create a new appdef file for the given executable. For more advanced");
      Gray(     "  uses you probably need to edit it before use.");
      GreenGray("  -F", "Enables overwriting an existing appdef");

      return 0;
    }

    static int CmdList(RunOptions o)
    {

      return 0;
    }
    
    static int Main(string[] args)
    {
      try
      {
        _options.Initialize(args);

        switch(_options.Action)
        {
          case "run":
          case "list":
          case "register":
            throw new NotImplementedException($"Not yet implemented: action '{_options.Action}'");
          case "help":
            return ShowHelp(_options);
          default:
            throw new InvalidOperationException($"Unrecognized action '{_options.Action}'");
        }

        //throw new InvalidOperationException(
        //  "This is a test",
        //    new InvalidOperationException(
        //      "Inner exception test"));
      }
      catch(Exception ex)
      {
        Console.WriteLine();
        if(_options.Verbose)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.Write($"{ex}");
          Console.ResetColor();
          Console.WriteLine();
        }
        else
        {
          var e = ex;
          var indent = "";
          while(e != null)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{e.GetType().FullName}: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{e.Message}");
            e = e.InnerException;
            indent += "  ";
            if(e != null)
            {
              Console.ForegroundColor = ConsoleColor.Blue;
              Console.Write($"{indent}--> ");
            }
            Console.ResetColor();
          }
        }
        Console.ResetColor();
        Console.WriteLine();
        return 1;
      }
    }

  }
}
