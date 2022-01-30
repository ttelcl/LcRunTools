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

using Lcl.RunLib.ApplicationDefinitions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Lcl.RunLib
{
  public class AppDefTests
  {
    private readonly ITestOutputHelper _output;

    public AppDefTests(ITestOutputHelper outputHelper)
    {
      _output = outputHelper;
    }

    [Fact]
    public void CanLoadSingleSample()
    {
      var defname = "test-single";
      var cwd = Environment.CurrentDirectory;
      _output.WriteLine($"CWD = {cwd}");
      
      var store = new AppdefStore(cwd, null);
      var fnm = store.FindAppdefFile(defname);
      Assert.NotNull(fnm);
      fnm = TagAsNotNull(fnm);
      _output.WriteLine($"Found appdef in {fnm}");

      // "manual" loading (bypassing the helpers)
      var json = File.ReadAllText(fnm);
      var im = InvocationMutation.FromJson(json);
      Assert.NotNull(im);
      Assert.Null(im.BaseName);

    }

    [Fact]
    public void CanLoadAndDumpSingleSample()
    {
      var defname = "test-single";
      var cwd = Environment.CurrentDirectory;
      var store = new AppdefStore(cwd, null);
      var fnm = store.FindAppdefFile(defname);
      Assert.NotNull(fnm);
      fnm = TagAsNotNull(fnm);
      var json = File.ReadAllText(fnm);
      var im = InvocationMutation.FromJson(json);
      Assert.NotNull(im);

      // Dump the object for checking
      var json2 = im.ToJson();
      
      var name2 = defname + ".check.json";
      _output.WriteLine($"Saving {name2}");
      File.WriteAllText(name2, json2);

      // checking the dumped content by loading it as a JObject
      var jo = JsonConvert.DeserializeObject<JObject>(json2);
      Assert.NotNull(jo);
      jo = TagAsNotNull(jo);
      if(jo["tobase"] is JObject tobase)
      {
        Assert.False(tobase.ContainsKey("command"));
        Assert.False(tobase.ContainsKey("prepend-command-path"));
      }
      else
      {
        Assert.True(false, "missing 'tobase' object");
      }
      if(jo["frombase"] is JObject frombase)
      {
        Assert.True(frombase.ContainsKey("command"));
        Assert.True(frombase.ContainsKey("prepend-command-path"));
      }
      else
      {
        Assert.True(false, "missing 'frombase' object");
      }
    }

    [Fact]
    public void CanLoadSingleNodeChain()
    {
      var defname = "test-single";
      var cwd = Environment.CurrentDirectory;
      var store = new AppdefStore(cwd, null);

      var chain = store.LoadAppdef(defname);
      Assert.NotNull(chain);
      Assert.Null(chain.BaseNode);
      Assert.NotNull(chain.Content);
      Assert.Equal(defname, chain.Tag);

      _output.WriteLine($"Matched tag '{defname}' as file: {chain.FileName}");
    }

    [Fact]
    public void CanCreateInvocationModel()
    {
      var im = InvocationModel.CreateDefault(null);
      Assert.NotNull(im);
      _output.WriteLine($"There are {im.Variables.Count} environment variables");
      Assert.True(im.Variables.ContainsKey("PATH"));
      var path = im.Variables["PATH"];
      var pathlist = im.GetAsList("PATH", InvocationModel.DefaultListSeparator);
      Assert.NotNull(pathlist);
      _output.WriteLine($"There are {pathlist.Count} directories on the PATH");
      Assert.True(pathlist.Count > 4);
      im.SetAsList("PATH", InvocationModel.DefaultListSeparator, pathlist);
      var path2 = im.Variables["PATH"];
      Assert.Equal(path, path2);

      // serialize the model to a file for inspection
      var json = JsonConvert.SerializeObject(im, Formatting.Indented);
      var dumpname = "model-dump-1.json";
      _output.WriteLine($"Saving '{dumpname}'");
      File.WriteAllText(dumpname, json);
    }

    [Fact]
    public void CanBuildInvocationModel()
    {

      var defname = "test-single";
      var cwd = Environment.CurrentDirectory;
      var store = new AppdefStore(cwd, null);
      var chain = store.LoadAppdef(defname);
      var im = InvocationModel.CreateDefault(null);

      // serialize the pre-mutate model to a file for inspection
      var json = JsonConvert.SerializeObject(im, Formatting.Indented);
      var dumpname = "model-dump-2-pre-mutate.json";
      _output.WriteLine($"Saving '{dumpname}'");
      File.WriteAllText(dumpname, json);

      Assert.Null(im.Executable);
      Assert.DoesNotContain("-hide_banner", im.Arguments);
      Assert.DoesNotContain("FFMPEGPATH", im.Variables);
      
      chain.ApplyTo(im);

      // serialize the post-mutate model to a file for inspection
      json = JsonConvert.SerializeObject(im, Formatting.Indented);
      var dumpname2 = "model-dump-2-post-mutate.json";
      _output.WriteLine($"Saving '{dumpname2}'");
      File.WriteAllText(dumpname2, json);

      Assert.NotNull(im.Executable);
      Assert.Contains("-hide_banner", im.Arguments);
      Assert.Contains("FFMPEGPATH", im.Variables);
    }

    private static T TagAsNotNull<T>(T? t)
      where T : class
    {
      // Hopefully Xunit fixes Assert.NotNull some day
      // (see https://github.com/xunit/xunit/issues/2011#issuecomment-805197767 )
      if(t == null)
      {
        throw new ArgumentNullException(nameof(t));
      }
      return t;
    }

  }
}
