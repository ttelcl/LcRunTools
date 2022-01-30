/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Lcl.RunLib.ApplicationDefinitions
{
  /// <summary>
  /// Describes mutations to apply to an Invocation model during one of the
  /// two mutation phases
  /// </summary>
  public class InvocationMutationPhase
  {
    /// <summary>
    /// Create a new InvocationMutationPhase
    /// </summary>
    public InvocationMutationPhase(
      string? command = null,
      [JsonProperty("prepend-command-path")]
      bool? prependCommandPath = null,
      string? workdir = null,
      Dictionary<string, string?>? vars = null,
      Dictionary<string, ListVarMutation>? lists = null,
      ListMutation? args = null)
    {
      Command = command;
      PrependCommandPath = prependCommandPath;
      FullVariableMutations = vars ?? new Dictionary<string, string?>();
      ListVariableMutations = lists ?? new Dictionary<string, ListVarMutation>();
      ArgumentListMutations = args ?? new ListMutation();
      WorkDir = workdir;
    }

    /// <summary>
    /// The command
    /// </summary>
    [JsonProperty("command", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Command { get; }

    /// <summary>
    /// Whether or not the path of the executable should be automatically
    /// prepended to the PATH.
    /// </summary>
    [JsonProperty("prepend-command-path", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? PrependCommandPath { get; }

    /// <summary>
    /// The working directory, or null to not mutate it
    /// </summary>
    [JsonProperty("workdir", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? WorkDir { get; }

    /// <summary>
    /// Mutations to full variables
    /// </summary>
    [JsonProperty("vars")]
    public IReadOnlyDictionary<string, string?> FullVariableMutations { get; }

    /// <summary>
    /// Mutations variables that are treaated as list-valued
    /// </summary>
    [JsonProperty("lists")]
    public IReadOnlyDictionary<string, ListVarMutation> ListVariableMutations { get; }

    /// <summary>
    /// Mutations to the argument list
    /// </summary>
    [JsonProperty("args")]
    public ListMutation ArgumentListMutations { get; }

    /// <summary>
    /// Tell the JSON serializer whether or not to serialize the
    /// plain variable mutation map
    /// </summary>
    public bool ShouldSerializeFullVariableMutations()
    {
      return FullVariableMutations.Count > 0;
    }

    /// <summary>
    /// Tell the JSON serializer whether or not to serialize the
    /// list variable mutation map
    /// </summary>
    public bool ShouldSerializeListVariableMutations()
    {
      return ListVariableMutations.Count > 0;
    }

    /// <summary>
    /// Tell the JSON serializer whether or not to serialize the
    /// argument list mutations
    /// </summary>
    public bool ShouldSerializeArgumentListMutations()
    {
      return ArgumentListMutations.Prepend.Count + ArgumentListMutations.Append.Count > 0;
    }

    /// <summary>
    /// Applies the mutations defined in this phase to the model.
    /// Does not "finalize" the mutations (e.g.: does not prepend the command
    /// path if requested)
    /// </summary>
    public void ApplyTo(InvocationModel model)
    {
      if(Command != null)
      {
        if(model.Executable != null)
        {
          throw new InvalidOperationException(
            $"Duplicate definition of the command to excute: '{model.Executable}' and '{Command}'");
        }
        model.Executable = Command;
      }
      if(PrependCommandPath.HasValue)
      {
        model.PrependCommandPath = PrependCommandPath.Value;
      }

      // Processing a list variable may change an existing plain variable into a list variable
      // The reverse is not true. However, plain variables are processed first.

      foreach(var plainKvp in FullVariableMutations)
      {
        var varName = plainKvp.Key;
        var varValue = plainKvp.Value;
        model.Variables[varName] = varValue;
      }

      foreach(var listKvp in ListVariableMutations)
      {
        var varName = listKvp.Key;
        var lm = listKvp.Value;
        var list = model.GetAsList(varName, lm.Separator);
        lm.Apply(list);
        model.SetAsList(varName, lm.Separator, list);
      }

      ArgumentListMutations.Apply(model.Arguments);
    }
  }
}
