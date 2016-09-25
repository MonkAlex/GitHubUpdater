using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace GitHubUpdater.WPF
{
  public static class Helper
  {

    [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
    public static extern long StrFormatByteSize(long fileSize, System.Text.StringBuilder buffer, int bufferSize);

    public static string HumanizeByteSize(this long bytes)
    {
      var sb = new StringBuilder(11);
      StrFormatByteSize(bytes, sb, sb.Capacity);
      return sb.ToString();
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
          .Where(t => !t.IsAbstract && t.IsClass && typeof (T).IsAssignableFrom(t)));
      }
      return types;
    }

    public static IEnumerable<T> CreateAllTypes<T>(params object[] args)
    {
      return GetAllTypes<T>().Select(type => (T)Activator.CreateInstance(type, args));
    }
  }
}