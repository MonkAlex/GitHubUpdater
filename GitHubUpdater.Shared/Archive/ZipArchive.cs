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
    public FileInfo File { get; }

    public event EventHandler<UnpackExceptionEventArgs> ExceptionThrowed;

    protected bool IsValud()
    {
      return File.Exists && Extension.Contains(File.Extension);
    }

    public bool Unpack(string folder, bool unpackRootSubfolder, IProgress<UnpackProgress> progress)
    {
      if (!IsValud())
        return false;

      using (var zipFile = ZipFile.OpenRead(File.FullName))
      {
        var entries = zipFile.Entries.ToList();
        var subfolder = string.Empty;

        if (unpackRootSubfolder)
        {
          var files = entries.Select(e => e.FullName).ToList();
          subfolder = files.OrderBy(f => f.Length).Select(Path.GetDirectoryName).FirstOrDefault();
          while (subfolder != null)
          {
            var count = files.GroupBy(f => f.StartsWith(subfolder)).Count();
            if (count == 1)
              break;
            subfolder = Path.GetDirectoryName(subfolder);
          }
        }

        foreach (var entry in entries)
        {
          if (progress != null)
          {
            var unpack = new UnpackProgress();
            unpack.ProgressPercentage = (int)(entries.IndexOf(entry) * 100.00 / entries.Count);
            unpack.LastFile = entry.Name;
            progress.Report(unpack);
          }
          ExceptionHandler.TryExecute(() =>
          {
            var fixedName = Regex.Replace(entry.FullName, string.Format("^{0}//*", subfolder), string.Empty,
              RegexOptions.IgnoreCase);
            var fullPath = Path.Combine(folder, fixedName);
            if (Path.GetFileName(fullPath).Length == 0)
            {
              if (entry.Length == 0L)
                Directory.CreateDirectory(fullPath);
            }
            else
            {
              Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
              var needExtract = true;
              if (System.IO.File.Exists(fullPath))
              {
                using (var content = new MemoryStream())
                using (var entryStream = entry.Open())
                using (var file = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
                {
                  entryStream.CopyTo(content);
                  byte[] onDisk = new byte[file.Length];
                  file.Read(onDisk, 0, (int)file.Length);
                  if (onDisk.SequenceEqual(content.ToArray()))
                  {
                    needExtract = false;
                    this.Debug($"File {fullPath} already exists and no changes found.");
                  }
                }
              }
              if (needExtract)
                entry.ExtractToFile(fullPath, true);
            }
          }, OnExceptionThrowed);
        }
      }
      return true;
    }

    public bool Test()
    {
      if (!IsValud())
        return false;

      try
      {
        using (var zipFile = ZipFile.OpenRead(File.FullName))
        {
          foreach (var entry in zipFile.Entries)
          {
            using (var destination = new MemoryStream())
            {
              using (Stream stream = entry.Open())
                stream.CopyTo(destination);
              return destination.Length == entry.Length;
            }
          }
        }
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public ZipArchive(string file)
    {
      File = new FileInfo(file);
    }

    protected virtual UpdateExceptionReaction OnExceptionThrowed(Exception ex)
    {
      var reaction = new UnpackExceptionEventArgs(ex);
      ExceptionThrowed?.Invoke(this, reaction);
      return reaction.Handled;
    }
  }
}