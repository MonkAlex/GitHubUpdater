using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitHubUpdater.Shared.Archive
{
  public class ZipArchive : IArchive
  {
    public string[] Extension { get { return new[] { ".zip", ".cbz" }; } }
    public FileInfo File { get; set; }

    protected bool IsValud()
    {
      return File.Exists && Extension.Contains(File.Extension);
    }

    public void Unpack(string folder)
    {
      if (!IsValud())
        return;

      using (var zipFile = ZipFile.OpenRead(File.FullName))
      {
        zipFile.ExtractToDirectory(folder);
      }
    }

    public void Unpack(string folder, string subfolder)
    {
      if (!IsValud())
        return;

      using (var zipFile = ZipFile.OpenRead(File.FullName))
      {
        foreach (var entry in zipFile.Entries.Where(e => e.FullName.StartsWith(subfolder)))
        {
          ExceptionHandler.TryExecute(() =>
          {
            var fixedName = Regex.Replace(entry.FullName, string.Format("^{0}//*", subfolder), string.Empty, RegexOptions.IgnoreCase);
            var fullPath = Path.Combine(folder, fixedName);
            if (Path.GetFileName(fullPath).Length == 0)
            {
              if (entry.Length == 0L)
                Directory.CreateDirectory(fullPath);
            }
            else
            {
              Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
              entry.ExtractToFile(fullPath, true);
            }
          });
        }
      }
    }

    public bool Test()
    {
      if (!IsValud())
        return false;

      using (var zipFile = ZipFile.OpenRead(File.FullName))
      {
        foreach (var entry in zipFile.Entries)
        {
          var tested = ExceptionHandler<bool>.TryExecute(() =>
          {
            using (var destination = new MemoryStream())
            {
              using (Stream stream = entry.Open())
                stream.CopyTo(destination);
              return destination.Length == entry.Length;
            }
          }, false);
          if (!tested)
            return false;
        }
      }
      return true;
    }

    public ZipArchive(string file)
    {
      this.File = new FileInfo(file);
    }
  }
}