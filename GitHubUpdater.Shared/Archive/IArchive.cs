﻿using System;
using System.Reflection.Emit;
using System.Threading.Tasks;

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
    /// <returns>True, if unpack completed.</returns>
    bool Unpack(string folder);

    /// <summary>
    /// Unpack to folder.
    /// </summary>
    /// <param name="folder">Target folder.</param>
    /// <param name="subfolder">Folder in archive for unpack.</param>
    /// <returns>True, if unpack completed.</returns>
    bool Unpack(string folder, string subfolder);

    /// <summary>
    /// Test archive.
    /// </summary>
    /// <returns>True, if archive valid.</returns>
    bool Test();
  }
}