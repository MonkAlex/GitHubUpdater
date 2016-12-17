using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace GitHubUpdater.Shared
{
  public static class Log
  {
    public static void Debug(this object @from, object message)
    {
      LogManager.GetLogger(@from.GetType().Name).Debug(message);
    }

    public static void Debug(this Type @from, object message)
    {
      LogManager.GetLogger(@from.Name).Debug(message);
    }

    public static void Error(this object @from, object message)
    {
      LogManager.GetLogger(@from.GetType().Name).Error(message);
    }

    public static void Error(this Type @from, object message)
    {
      LogManager.GetLogger(@from.Name).Error(message);
    }

    static Log()
    {
      var config = new LoggingConfiguration();
      var fileTarget = new FileTarget();
      config.AddTarget("file", fileTarget);
      fileTarget.FileName = @"${basedir}/${processname}_${date:format=yyyy_MM_dd}.log";
      fileTarget.Layout = @"${date:format=HH\:mm\:ss} ${processid} ${threadid} ${logger} ${message}";
      var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
      config.LoggingRules.Add(rule2);
      LogManager.Configuration = config;
    }
  }
}
