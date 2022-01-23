/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
    private readonly Dictionary<string, char> _listSeparators =
      new Dictionary<string, char>(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, List<string>> _listVariables
      = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, string> _plainVariables
      = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Create a new InvocationModel
    /// </summary>
    /// <param name="arguments">
    /// The initial arguments
    /// </param>
    public InvocationModel(IEnumerable<string> arguments)
    {
      DefaultListSeparator =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
      Executable = null;
      PrependCommandPath = true;
      Arguments = new List<string>(arguments);
      WorkingDirectory = Environment.CurrentDirectory;
      _listSeparators["PATH"] = DefaultListSeparator;
    }

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
    /// The default list separator character (depending on operating system)
    /// </summary>
    public char DefaultListSeparator { get; }

    /// <summary>
    /// The mapping from environment variable names to list separator characters
    /// for list-valued environment variables. Being listed in this ditctionary
    /// implies the variable should be treated as a list.
    /// Initially this contains an entry specifying the separator for the
    /// PATH variable to be the DefaultListSeparator.
    /// </summary>
    public IReadOnlyDictionary<string, char> ListSeparators { get => _listSeparators; }

    /// <summary>
    /// Environment variables that are to be treated as lists. Each entry must have a matching
    /// entry in ListSeparators, and no matching entry in PlainVariables
    /// </summary>
    public IReadOnlyDictionary<string, List<string>> ListVariables { get => _listVariables; }

    /// <summary>
    /// Environment variables that are to be treated as plain strings, not lists.
    /// Entries must *not* also match entries in ListSeparators and ListVariables
    /// </summary>
    public IReadOnlyDictionary<string, string> PlainVariables { get => _plainVariables; }

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
    /// Declare a variable as list variable and define its separator. Throws an exception
    /// if a conflicting separator was defined before. If a plain variable by this name
    /// exists, it is split and migrated to become a list variable
    /// </summary>
    /// <param name="name">
    /// The name of the variable
    /// </param>
    /// <param name="separator">
    /// The separator to use. Currently only ':', ';', ',', ' ' and '|' are allowed
    /// </param>
    public void DeclareList(string name, char separator)
    {
      if(":;, |".IndexOf(separator) < 0)
      {
        throw new ArgumentException(
          $"Invalid value for 'separator'", nameof(separator));
      }
      if(_listSeparators.TryGetValue(name, out var oldsep))
      {
        if(oldsep != separator)
        {
          throw new InvalidOperationException(
            $"Conflicting list separator declaration for '{name}': cannot redeclare " +
            $"as '{separator}' after previously declaring as '{oldsep}'");
        }
        // else: NOP
      }
      else
      {
        _listSeparators[name] = separator;
      }
      if(_plainVariables.TryGetValue(name, out var value))
      {
        _listVariables[name] = value.Split(separator).ToList();
        _plainVariables.Remove(name);
      }
      // else: leave _listVariables[name] uninitialized or unmodified
    }

    /// <summary>
    /// Shorthand for DeclareList(name, DefaultListSeparator);
    /// </summary>
    public void DeclareList(string name)
    {
      DeclareList(name, DefaultListSeparator);
    }

    /// <summary>
    /// Set, replace or remove a plain or list variable. If the variable is known
    /// to be a list, the value is split according to the separator
    /// </summary>
    /// <param name="name">
    /// The name of the variable
    /// </param>
    /// <param name="value">
    /// The new value of the plain variable, the serialized form of the new value
    /// of the the list variable, or null to delete the variable.
    /// </param>
    public void SetVariable(string name, string? value)
    {
      if(_listSeparators.TryGetValue(name, out var separator))
      {
        if(value == null)
        {
          _listVariables.Remove(name);
        }
        else
        {
          _listVariables[name] = value.Split(separator).ToList();
        }
      }
      else
      {
        if(value == null)
        {
          _plainVariables.Remove(name);
        }
        else
        {
          _plainVariables[name] = value;
        }
      }
    }

    /// <summary>
    /// Set, replace, or delete the value of a list variable
    /// </summary>
    /// <param name="name">
    /// The name of the list variable
    /// </param>
    /// <param name="values">
    /// The new list of variables, or null to delete the variable
    /// </param>
    public void SetListVariable(string name, IEnumerable<string>? values)
    {
      var _ = RequireSeparator(name);
      if(values == null)
      {
        _listVariables.Remove(name);
      }
      else
      {
        _listVariables[name] = new List<string>(values);
      }
    }

    private char RequireSeparator(string name)
    {
      if(_listSeparators.TryGetValue(name, out var separator))
      {
        return separator;
      }
      else
      {
        throw new InvalidOperationException(
          $"The variable '{name}' is used as list variable but no list separator has been declared for it");
      }
    }


  }
}
