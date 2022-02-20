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
  /// Describes a collection of mutations to be applied to a 
  /// list-valued item (argument list or variable)
  /// </summary>
  public class ListMutation
  {
    private readonly List<string> _prepend;
    private readonly List<string> _append;

    /// <summary>
    /// Create a new ListMutation
    /// </summary>
    public ListMutation(
      IEnumerable<string>? prepend = null,
      IEnumerable<string>? append = null
      )
    {
      _prepend = new List<string>();
      _append = new List<string>();
      Prepend = _prepend.AsReadOnly();
      Append = _append.AsReadOnly();
      if(prepend != null)
      {
        _prepend.AddRange(prepend);
      }
      if(append != null)
      {
        _append.AddRange(append);
      }
    }

    /// <summary>
    /// Elements to be prepended to the variable list
    /// </summary>
    [JsonProperty("prepend")]
    public IReadOnlyList<string> Prepend { get; }

    /// <summary>
    /// Elements to be appended to the variable list
    /// </summary>
    [JsonProperty("append")]
    public IReadOnlyList<string> Append { get; }

    /// <summary>
    /// Tells the JSON serializer to skips serialization of "prepend" if it is empty
    /// </summary>
    public bool ShouldSerializePrepend()
    {
      return SerializeVerbose || Prepend.Any();
    }

    /// <summary>
    /// Tells the JSON serializer to skips serialization of "append" if it is empty
    /// </summary>
    public bool ShouldSerializeAppend()
    {
      return SerializeVerbose || Append.Any();
    }

    /// <summary>
    /// When set to true, the 'append' and 'prepend' fields are serialized even when empty
    /// </summary>
    [JsonIgnore]
    public bool SerializeVerbose { get; set; }

    /// <summary>
    /// Apply the mutations defined in this object to the given list
    /// </summary>
    public void Apply(List<string> list)
    {
      list.InsertRange(0, _prepend);
      list.InsertRange(list.Count, _append);
    }

  }
}
