/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lcl.RunLib.ApplicationDefinitions
{
  /// <summary>
  /// A node (link) in a linked chain of InvocationMutations
  /// </summary>
  public class InvocationMutationNode
  {
    /// <summary>
    /// Create a new InvocationMutationNode. Invoked by AppdefStore
    /// </summary>
    internal InvocationMutationNode(
      InvocationMutationNode? baseNode,
      string fileName,
      string tag,
      InvocationMutation content)
    {
      BaseNode = baseNode;
      FileName = fileName;
      Tag = tag;
      Content = content;
      if(!AppdefStore.IsValidApptag(tag))
      {
        throw new ArgumentException(
          "Invalid apptag (it contains invalid characters)",
          nameof(tag));
      }
    }

    /// <summary>
    /// The parent (base) node (possibly null)
    /// </summary>
    public InvocationMutationNode? BaseNode { get; }

    /// <summary>
    /// The full filename of the file the content is read from
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// The tag identifying the source file
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// The actual InvocationMutation (JSON serializable)
    /// </summary>
    public InvocationMutation Content { get; }

    /// <summary>
    /// True if this node has a parent, false if it is the base node
    /// </summary>
    public bool HasBase { get => BaseNode != null; }

    /// <summary>
    /// Applies this mutation to the model: the ToBasePhase, the base mutation
    /// (if any) and the FromBasePhase. 
    /// </summary>
    /// <param name="model">
    /// The model to Mutate
    /// </param>
    public void ApplyTo(InvocationModel model)
    {
      Content.ToBasePhase.ApplyTo(model);
      BaseNode?.ApplyTo(model);
      Content.FromBasePhase.ApplyTo(model);
    }

  }
}
