/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lcl.RunLib.ApplicationDefinitions
{
  /// <summary>
  /// Editable model for the parameters of a process invocation.
  /// "Processing" an application definition file comes down to editing an
  /// instance of this class.
  /// </summary>
  public class InvocationModel
  {
    private static readonly StringComparer __varNameComparer =
      RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
      ? StringComparer.OrdinalIgnoreCase
      : StringComparer.Ordinal;
    private readonly Dictionary<string, char> _listSeparators =
      new Dictionary<string, char>(__varNameComparer);
    private readonly Dictionary<string, string?> _variables
      = new Dictionary<string, string?>(__varNameComparer);

    /// <summary>
    /// Create a new InvocationModel. Only the arguments are populated and
    /// "PATH" is tagged as using the system default separator.
    /// Environment variables are not imported.
    /// Use the Create() pseudoconstructore to also import environment variables
    /// </summary>
    /// <param name="arguments">
    /// The initial arguments
    /// </param>
    public InvocationModel(IEnumerable<string>? arguments)
    {
      Executable = null;
      PrependCommandPath = true;
      Arguments = arguments == null ? new List<string>() : new List<string>(arguments);
      WorkingDirectory = Environment.CurrentDirectory;
      _listSeparators["PATH"] = DefaultListSeparator;
    }

    /// <summary>
    /// Create a new InvocationModel wrapping the specified initial argument list
    /// and importing the current process' environment variables.
    /// </summary>
    public static InvocationModel CreateDefault(IEnumerable<string>? arguments)
    {
      var im = new InvocationModel(arguments);
      var e = Environment.GetEnvironmentVariables();
      foreach(System.Collections.DictionaryEntry kvp in e)
      {
        im.Variables[(string)kvp.Key] = (string?)kvp.Value;
      }
      return im;
    }

    /// <summary>
    /// The comparer for environment variable names. Case insensitive on windows
    /// and case sensitive on non-windows systems
    /// </summary>
    [JsonIgnore]
    public static StringComparer VariableNameComparer { get => __varNameComparer;}
    
    /// <summary>
    /// The default list separator character (depending on operating system)
    /// </summary>
    public static char DefaultListSeparator { get; } = 
      RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';

    /// <summary>
    /// The full path to the executable to run, or null if not yet defined
    /// </summary>
    public string? Executable { get; set; }

    /// <summary>
    /// Whether or not to automatically prepend the directory of the executable
    /// to the PATH. Default true.
    /// </summary>
    public bool PrependCommandPath { get; set; }

    /// <summary>
    /// The mapping from environment variable names to list separator characters
    /// for list-valued environment variables. Being listed in this ditctionary
    /// implies the variable should be treated as a list.
    /// Initially this contains an entry specifying the separator for the
    /// PATH variable to be the DefaultListSeparator.
    /// </summary>
    public IReadOnlyDictionary<string, char> ListSeparators { get => _listSeparators; }

    /// <summary>
    /// The environment variables. A value that is null explicitly indicates a deleted
    /// variable.
    /// </summary>
    public IDictionary<string, string?> Variables { get => _variables; }

    /// <summary>
    /// The list of arguments
    /// </summary>
    public List<string> Arguments { get; }

    /// <summary>
    /// The initial working directory for the process to spawn. 
    /// Initially this is the current directory at the time of constructing this object
    /// </summary>
    public string WorkingDirectory { get; set; }

    /// <summary>
    /// Tag the named variable as using the specified list separator and return
    /// the current effective value of the variable as a list.
    /// </summary>
    /// <param name="varname">
    /// The name of the variable. Case-sensitivity depends on the operating system.
    /// </param>
    /// <param name="separator">
    /// The list separator to use for this variable.
    /// Must be one of ':', ';', ',', ' ', or '|'.
    /// </param>
    /// <returns>
    /// If the variable did not exist or was empty: an empty list.
    /// Otherwise: the value of the variable split into a list by the specified
    /// separator
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the separator is not one of ':', ';', ',', ' ', or '|'
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the separator conflicts with a previously specified separator for the
    /// same variable
    /// </exception>
    public List<string> GetAsList(string varname, char separator)
    {
      if(":;, |".IndexOf(separator) < 0)
      {
        throw new ArgumentException(
          $"Invalid value for 'separator'", nameof(separator));
      }
      if(_listSeparators.TryGetValue(varname, out var oldsep))
      {
        if(oldsep != separator)
        {
          throw new InvalidOperationException(
            $"Conflicting list separator declaration for '{varname}': cannot redeclare " +
            $"as '{separator}' after previously declaring as '{oldsep}'");
        }
        // else: NOP
      }
      else
      {
        _listSeparators[varname] = separator;
      }
      if(_variables.TryGetValue(varname, out var value) && !String.IsNullOrEmpty(value))
      {
        return value.Split(separator).ToList();
      }
      else
      {
        return new List<string>();
      }
    }

    /// <summary>
    /// Set an environment variable to a value constructed from a list using the
    /// specified separator
    /// </summary>
    /// <param name="varname">
    /// The name of the variable (case-sensitivity depends on operating system)
    /// </param>
    /// <param name="separator">
    /// The separator to use
    /// </param>
    /// <param name="list">
    /// The sequence of values to store. Or null to mark the variable for deletion
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the separator is not one of ':', ';', ',', ' ', or '|'
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the separator conflicts with a previously specified separator for the
    /// same variable
    /// </exception>
    public void SetAsList(string varname, char separator, IEnumerable<string>? list)
    {
      if(":;, |".IndexOf(separator) < 0)
      {
        throw new ArgumentException(
          $"Invalid value for 'separator'", nameof(separator));
      }
      if(_listSeparators.TryGetValue(varname, out var oldsep))
      {
        if(oldsep != separator)
        {
          throw new InvalidOperationException(
            $"Conflicting list separator declaration for '{varname}': cannot redeclare " +
            $"as '{separator}' after previously declaring as '{oldsep}'");
        }
        // else: NOP
      }
      else
      {
        _listSeparators[varname] = separator;
      }
      if(list == null)
      {
        _variables[varname] = null;
      }
      else
      {
        var value = String.Join(separator, list);
        _variables[varname] = value;
      }
    }

    /// <summary>
    /// Finish all the values represented by this model: perform checks,
    /// apply PrependCommandPath
    /// </summary>
    public void Finish()
    {
      if(String.IsNullOrEmpty(Executable))
      {
        throw new InvalidOperationException("No command specified.");
      }
      if(!Path.IsPathFullyQualified(Executable))
      {
        throw new InvalidOperationException(
          $"The command path must be fully specified");
      }
      if(PrependCommandPath)
      {
        var cmdpath = Path.GetDirectoryName(Executable);
        if(cmdpath == null)
        {
          throw new InvalidOperationException(
            "Internal error");
        }
        var pathlist = GetAsList("PATH", DefaultListSeparator);
        pathlist.Insert(0, cmdpath);
      }
      var listNames = ListSeparators.Keys.ToList();
    }


  }
}
