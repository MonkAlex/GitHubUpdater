using System;

namespace GitHubUpdater.Shared.Archive
{
  public interface IArchive
  {
    /// <summary>
    /// Supported extension.
    /// </summary>
    string[] Extension { get; }

    /// <summary>
    /// Event to show user error.
    /// </summary>
    event EventHandler<UnpackExceptionEventArgs> ExceptionThrowed;

    /// <summary>
    /// Unpack to folder.
    /// </summary>
    /// <param name="folder">Target folder.</param>
    /// <param name="unpackRootSubfolder">Unpack subfolder, if this single on root.</param>
    /// <param name="progress">Callback for progress.</param>
    /// <returns>True, if unpack completed.</returns>
    bool Unpack(string folder, bool unpackRootSubfolder, IProgress<UnpackProgress> progress);

    /// <summary>
    /// Test archive.
    /// </summary>
    /// <returns>True, if archive valid.</returns>
    bool Test();
  }
}