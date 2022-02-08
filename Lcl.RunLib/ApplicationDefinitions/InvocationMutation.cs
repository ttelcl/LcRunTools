/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Lcl.RunLib.ApplicationDefinitions
{
  /// <summary>
  /// Describes the set of mutations to an InvocationModel as
  /// specified in one single application definition file
  /// </summary>
  public class InvocationMutation
  {
    /// <summary>
    /// Create a new InvocationMutation
    /// </summary>
    public InvocationMutation(
      [JsonProperty("base")]
      string? baseName, // no default null in this case!!
      [JsonProperty("tobase")]
      InvocationMutationPhase toBase,
      [JsonProperty("frombase")]
      InvocationMutationPhase fromBase,
      string? description = null
      )
    {
      BaseName = baseName;
      ToBasePhase = toBase;
      FromBasePhase = fromBase;
      Description = description;
      if(toBase == null)
      {
        throw new ArgumentNullException(nameof(toBase), "'toBase' section missing");
      }
      if(fromBase == null)
      {
        throw new ArgumentNullException(nameof(fromBase), "'fromBase' section missing");
      }
    }

    /// <summary>
    /// Deserialize the content of an appdef
    /// </summary>
    /// <param name="json">
    /// The JSON content to deserialize
    /// </param>
    /// <returns>
    /// A new InvocationMutation object
    /// </returns>
    public static InvocationMutation FromJson(string json)
    {
      var im = JsonConvert.DeserializeObject<InvocationMutation>(json);
      if(im == null)
      {
        throw new InvalidOperationException(
          $"Invalid JSON content");
      }
      return im;
    }

    /// <summary>
    /// Serialize this object to a JSON string (an appdef)
    /// </summary>
    public string ToJson()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    /// <summary>
    /// The short name for the base application definition file, or
    /// null if there is none
    /// </summary>
    [JsonProperty("base")]
    public string? BaseName { get; }

    /// <summary>
    /// The mutations to the invocation model to be processed during the first
    /// processing phase
    /// </summary>
    [JsonProperty("tobase")]
    public InvocationMutationPhase ToBasePhase { get; }

    /// <summary>
    /// The mutations to the invocation model to be processed during the second
    /// processing phase
    /// </summary>
    [JsonProperty("frombase")]
    public InvocationMutationPhase FromBasePhase { get; }

    /// <summary>
    /// The (optional) brief description.
    /// </summary>
    [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Description { get; }

    /// <summary>
    /// Return the command defined in this instance (in frombase or tobase phase),
    /// if any. Not serialized here.
    /// </summary>
    [JsonIgnore]
    public string? Command {
      get {
        return FromBasePhase.Command ?? ToBasePhase.Command;
      }
    }

  }
}
