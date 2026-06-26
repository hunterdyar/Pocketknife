using System;
using System.Reflection;
using System.IO;
var dll = @"C:\Users\bloops\.nuget\packages\qtgroup.qt.bridge.csharp.win-x64\0.3.0-beta\lib\net8.0\Qt.Bridge.CSharp.Api.dll";
var dir = Path.GetDirectoryName(dll);
var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
var paths = new System.Collections.Generic.List<string>();
paths.AddRange(Directory.GetFiles(dir, "*.dll"));
paths.AddRange(Directory.GetFiles(runtimeDir, "*.dll"));
var resolver = new PathAssemblyResolver(paths);
using var mlc = new MetadataLoadContext(resolver);
var a = mlc.LoadFromAssemblyPath(dll);
foreach (var t in a.GetExportedTypes()) {
  if (t.Name == "Qml" || t.Name.StartsWith("QmlElement") || t.Namespace == "Qt.Quick" || t.Namespace == "Qt.MetaObject") {
    Console.WriteLine("== " + t.FullName);
    foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly)) {
      var ps = string.Join(", ", System.Linq.Enumerable.Select(m.GetParameters(), p => p.ParameterType.Name + " " + p.Name));
      Console.WriteLine("  " + m.ReturnType.Name + " " + m.Name + "(" + ps + ")");
    }
  }
}
