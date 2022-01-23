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
  /// list-valued variable. This is the in-memory model corresponding
  /// to a list-variable-edit part of an appdef file
  /// </summary>
  public class ListVarMutation: ListMutation
  {

    /// <summary>
    /// Create a new ListVarMutation
    /// </summary>
    public ListVarMutation(
      [JsonProperty("sep")] char separator,
      IEnumerable<string>? prepend = null,
      IEnumerable<string>? append = null
      ) : base(prepend, append)
    {
      Separator = separator;
    }

    /// <summary>
    /// The separator to use
    /// </summary>
    [JsonProperty("sep")]
    public char Separator { get; }

  }
}
