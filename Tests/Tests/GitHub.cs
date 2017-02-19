using System;
using System.Linq;
using GitHubUpdater.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
  [TestClass]
  public class GitHub
  {
    [TestMethod]
    public void GetReleases()
    {
      var option = new Option() { RepositoryId = 66179868 };
      var du = new DownloadUpdate(option);
      var hu = du.HasUpdate();
      Assert.IsTrue(hu.Result);
      var files = du.GetFiles();
      var hasFiles = files.Result.Any();
      Assert.IsTrue(hasFiles);
    }
  }
}
