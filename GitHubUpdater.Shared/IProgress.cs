namespace GitHubUpdater.Shared
{
  public interface IProgress
  {
    int ProgressPercentage { get; set; }
  }

  public class DownloadProgress : IProgress
  {
    public long BytesReceived { get; set; }

    public long TotalBytesToReceive { get; set; }

    public int ProgressPercentage { get; set; }
  }

  public class UnpackProgress : IProgress
  {
    public int ProgressPercentage { get; set; }

    public string LastFile { get; set; }
  }
}