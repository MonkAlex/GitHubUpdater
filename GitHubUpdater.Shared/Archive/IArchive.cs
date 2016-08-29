namespace GitHubUpdater.Shared.Archive
{
  public interface IArchive
  {
    /// <summary>
    /// Supported extension.
    /// </summary>
    string[] Extension { get; }

    /// <summary>
    /// Unpack to folder.
    /// </summary>
    /// <param name="folder">Target folder.</param>
    void Unpack(string folder);

    /// <summary>
    /// Unpack to folder.
    /// </summary>
    /// <param name="folder">Target folder.</param>
    /// <param name="subfolder">Folder in archive for unpack.</param>
    void Unpack(string folder, string subfolder);

    /// <summary>
    /// Test archive.
    /// </summary>
    /// <returns>True, if archive valid.</returns>
    bool Test();
  }
}