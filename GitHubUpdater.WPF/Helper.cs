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
}