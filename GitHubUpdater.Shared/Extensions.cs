using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHubUpdater.Shared
{
  public static class Helper
  {
    public static string HumanizeByteSize(this long byteCount)
    {
      string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
      if (byteCount == 0)
        return "0" + suf[0];
      long bytes = Math.Abs(byteCount);
      int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
      double num = Math.Round(bytes / Math.Pow(1024, place), 1);
      return Math.Sign(byteCount) * num + suf[place];
    }
  }

  public static class Generic
  {
    public static List<Type> GetAllTypes<T>()
    {
      var types = new List<Type>();
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        types.AddRange(assembly.GetTypes()
          .Where(t => !t.IsAbstract && t.IsClass && typeof(T).IsAssignableFrom(t)));
      }
      return types;
    }

    public static IEnumerable<T> CreateAllTypes<T>(params object[] args)
    {
      return GetAllTypes<T>().Select(type => (T)Activator.CreateInstance(type, args));
    }
  }
}