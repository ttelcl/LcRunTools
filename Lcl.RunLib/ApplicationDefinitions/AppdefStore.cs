/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lcl.RunLib.ApplicationDefinitions
{
  /// <summary>
  /// Abstract identifiers for standard AppdefStore setups
  /// </summary>
  public enum StoreLocation
  {
    /// <summary>
    /// An AppdefStore including just the system folder
    /// DEPRECATED
    /// </summary>
    System,

    /// <summary>
    /// An AppdefStore using the user folder
    /// </summary>
    User,

    /// <summary>
    /// An AppdefStore using the current working directory, with the user folder
    /// as fallback
    /// </summary>
    Local,
  }

  /// <summary>
  /// A logical storage for application definition files. Implemented
  /// as a specialized view on a folder
  /// </summary>
  public class AppdefStore
  {
    /// <summary>
    /// Create a new AppdefStore that is backed by a specified folder,
    /// and may have a parent store
    /// </summary>
    public AppdefStore(
      string folder,
      string label,
      bool quiet = false,
      AppdefStore? parent = null)
    {
      Folder = Path.GetFullPath(folder);
      Parent = parent;
      Label = label;
      Quiet = quiet;
    }

    /// <summary>
    /// The folder providing the backing for this store. This folder may be
    /// non-existent
    /// </summary>
    public string Folder { get; }

    /// <summary>
    /// True if the folder exists
    /// </summary>
    public bool Exists { get => Directory.Exists(Folder); }

    /// <summary>
    /// A label to identify this particular store
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// The parent store, if any
    /// </summary>
    public AppdefStore? Parent { get; }

    /// <summary>
    /// When true: some commands stay should silent about this store if it does not exist
    /// </summary>
    public bool Quiet { get; }

    /// <summary>
    /// The file extension for appdef files
    /// </summary>
    public const string AppdefExtension = ".appdef2.json";

    /// <summary>
    /// Retrieve the default store instance identified by the location code
    /// </summary>
    public static AppdefStore DefaultStore(StoreLocation location)
    {
      switch(location)
      {
        case StoreLocation.System:
          throw new NotSupportedException("The system store is not enabled in this version");
        //return AppdefStore.DefaultSystemStoreOld;
        case StoreLocation.User:
          return AppdefStore.DefaultUserStore;
        case StoreLocation.Local:
          return AppdefStore.DefaultLocalStore;
        default:
          throw new ArgumentOutOfRangeException(
            nameof(location), "Unrecognized appdef store identifier");
      }
    }

    /// <summary>
    /// The AppdefStore exposing the system wide application definitions.
    /// (deprecated)
    /// </summary>
    public static AppdefStore DefaultSystemStoreOld { get; }
      = new AppdefStore(SystemStoreFolder, "system", true, null);

    /// <summary>
    /// The AppdefStore exposing the current user's application definitions
    /// and using the old system store as fallback (deprecated)
    /// </summary>
    public static AppdefStore DefaultUserStoreOld { get; }
      = new AppdefStore(UserStoreFolder, "user", false, DefaultSystemStoreOld);

    /// <summary>
    /// The AppDefStore exposing application definitions in the current directory
    /// and using the old user store as fallback (and the old system store as further
    /// fallback)
    /// (deprecated)
    /// </summary>
    public static AppdefStore DefaultLocalStoreOld { get; }
      = new AppdefStore(Environment.CurrentDirectory, "local", true, DefaultUserStoreOld);

    /// <summary>
    /// The AppdefStore exposing the current user's application definitions
    /// and not using any fallback
    /// </summary>
    public static AppdefStore DefaultUserStore { get; }
      = new AppdefStore(UserStoreFolder, "user", false, null);

    /// <summary>
    /// The AppDefStore exposing application definitions in the current directory
    /// and using the user store as fallback
    /// </summary>
    public static AppdefStore DefaultLocalStore { get; }
      = new AppdefStore(Environment.CurrentDirectory, "local", true, DefaultUserStore);

    /// <summary>
    /// Get the name of the folder for the current user's application definitions.
    /// Note that this folder may be non-existent
    /// </summary>
    public static string UserStoreFolder {
      get {
        var folder = Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "Lcl",
          "Apprun",
          ".appdefs2");
        return folder;
      }
    }

    /// <summary>
    /// Get the name of the folder for the system application definitions.
    /// Note that this folder may be non-existent
    /// </summary>
    public static string SystemStoreFolder {
      get {
        var folder = Path.Combine(Environment.GetFolderPath(
          Environment.SpecialFolder.CommonApplicationData),
          "Lcl",
          "Apprun",
          ".appdefs2");
        return folder;
      }
    }

    private static char[] __badChars = "\\/: \t\n\r\f".ToCharArray();

    /// <summary>
    /// A simple check if the given string may be a valid apptag.
    /// If this returns false, it definitely isn't. If it returns true
    /// there is a good chance that it is valid (but not a strict guarantee)
    /// </summary>
    public static bool IsValidApptag(string apptag)
    {
      if(String.IsNullOrEmpty(apptag))
      {
        return false;
      }
      if(apptag.IndexOfAny(__badChars) >= 0)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Try to find an appdef and report the file name and store it was found in 
    /// if successfule
    /// </summary>
    /// <returns>
    /// True if successfull, false if not found
    /// </returns>
    public bool TryFindAppdefFile(string tag, bool recurse, out string? filename, out AppdefStore? store)
    {
      var fullname = AppdefFileName(tag);
      if(File.Exists(fullname))
      {
        filename = fullname;
        store = this;
        return true;
      }
      if(recurse && Parent != null)
      {
        return Parent.TryFindAppdefFile(tag, recurse, out filename, out store);
      }
      else
      {
        filename = null;
        store = null;
        return false;
      }
    }

    /// <summary>
    /// Find the full path to the application definition file indicated by the tag, or
    /// null if not found.
    /// </summary>
    /// <param name="tag">
    /// The application tag
    /// </param>
    /// <param name="recurse">
    /// Default true. If true, continue searching in the parent store if not found.
    /// </param>
    /// <returns>
    /// The full path of the file if found, or null if not found.
    /// </returns>
    public string? FindAppdefFile(string tag, bool recurse = true)
    {
      var fullname = AppdefFileName(tag);
      if(File.Exists(fullname))
      {
        return fullname;
      }
      if(recurse && Parent != null)
      {
        return Parent.FindAppdefFile(tag, recurse);
      }
      else
      {
        return null;
      }
    }

    /// <summary>
    /// Return the name that the application definition file would have if it
    /// existed in this store.
    /// </summary>
    /// <param name="tag">
    /// The application tag
    /// </param>
    /// <returns>
    /// The full name of the file.
    /// </returns>
    public string AppdefFileName(string tag)
    {
      if(String.IsNullOrEmpty(tag))
      {
        throw new ArgumentOutOfRangeException(nameof(tag),
          "Application tag cannot be null or empty");
      }
      if(tag.IndexOfAny(new[] { '/', '\\', ':' }) >= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(tag),
          "Application tag must not contain path separator characters");
      }
      var fullname = Path.Combine(Folder, tag + AppdefExtension);
      return fullname;
    }

    /// <summary>
    /// Enumerate all application definition tags found in this store.
    /// The enumeration only applies to this store, not to parents if they exist
    /// </summary>
    public IEnumerable<string> ListAppTags()
    {
      var di = new DirectoryInfo(Folder);
      if(di.Exists)
      {
        var files = di.GetFiles("*.appdef2.json");
        foreach(var file in files)
        {
          var tag = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file.Name));
          if(!String.IsNullOrEmpty(tag))
          // Otherwise a DoS could be done by triggering the case where GetFileNameWithoutExtension
          // returns null
          {
            yield return tag;
          }
        }
      }
    }

    /// <summary>
    /// Load an application definition by its tag, as well as all its base 
    /// application definitions, and return them as an 
    /// InvocationMutationNode (a linked list)
    /// </summary>
    /// <param name="tag">
    /// The tag of the main application definition
    /// </param>
    /// <returns>
    /// An InvocationMutationNode: effectively a linked list of InvocationMutation
    /// instances
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the application definition or any of its bases cannot be found, or
    /// if a cyclical dependency is encountered, or if an invalid tag is encountered.
    /// </exception>
    public InvocationMutationNode LoadAppdef(
      string tag)
    {
      var checkSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
      return LoadAppdefNode(tag, checkSet);
    }

    /// <summary>
    /// Load the indicated bare application definition from this store.
    /// Fails if there is no such appdef in this store.
    /// Does not use the search path, only checks this store itself.
    /// </summary>
    public InvocationMutation LoadAppdefBare(string tag)
    {
      var file = AppdefFileName(tag);
      if(!File.Exists(file))
      {
        throw new InvalidOperationException(
          $"Appdef file not found: {file}");
      }
      var json = File.ReadAllText(file);
      var im = InvocationMutation.FromJson(json);
      return im;
    }

    /// <summary>
    /// Enumerate the apptags for the application definitions locally in this store
    /// (does not recurse into parents)
    /// </summary>
    public IEnumerable<string> Apptags()
    {
      var di = new DirectoryInfo(Folder);
      if(di.Exists)
      {
        foreach(var fi in di.GetFiles("*"+AppdefExtension))
        {
          var name = fi.Name;
          var tag = name[..^AppdefExtension.Length];
          yield return tag;
        }
      }
    }

    private InvocationMutationNode LoadAppdefNode(string tag, HashSet<string> checkSet)
    {
      if(checkSet.Contains(tag))
      {
        throw new InvalidOperationException(
          $"Invalid application definition chain: '{tag}' appears more than once");
      }
      var file = FindAppdefFile(tag, false);
      if(file != null)
      {
        checkSet.Add(tag);
        var json = File.ReadAllText(file);
        var im = InvocationMutation.FromJson(json);
        if(String.IsNullOrEmpty(im.BaseName))
        {
          return new InvocationMutationNode(null, file, tag, im);
        }
        else
        {
          // recurse
          var baseNode = LoadAppdefNode(im.BaseName, checkSet);
          return new InvocationMutationNode(baseNode, file, tag, im);
        }
      }
      else
      {
        if(Parent != null)
        {
          // Note: it is intentional that once a parent store is used, further
          // base application definitions are only sought there (or its further
          // parent stores), but search never returns to this "child" store.
          return Parent.LoadAppdefNode(tag, checkSet);
        }
        else
        {
          throw new InvalidOperationException(
            $"Unresolved application definition: '{tag}'");
        }
      }
    }

  }
}
