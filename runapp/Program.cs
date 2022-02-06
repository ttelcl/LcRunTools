using System;

namespace Lcl.RunApp // Note: actual namespace depends on the project name.
{
  internal class Program
  {
    static RunOptions _options = new RunOptions();

    static int ShowHelp(RunOptions o)
    {
      Console.WriteLine("(TBD: show help message)");
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
