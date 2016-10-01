using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubUpdater.Launcher
{
  [Flags]
  public enum ExitCodes
  {
    None = 0,
    WebsiteOpened = 1,
    AnyVersionNotFound = 2,
    AnyExeFileNotFound = 4
  }
}
