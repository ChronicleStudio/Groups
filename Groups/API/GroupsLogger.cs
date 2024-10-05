
using System.IO;
using Vintagestory;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Groups.API
{
	public class GroupsLogger : Logger
	{
		public GroupsLogger(bool clearOldFiles) : base("Groups", clearOldFiles)
		{
		}

		public override string getLogFile(EnumLogType logType)
		{
			return Path.Combine(GamePaths.Logs, "server-groups.txt");
			/*
			switch (logType)
			{
				case EnumLogType.Audit:
					return Path.Combine(GamePaths.Logs, "groups-audit.txt");
				case EnumLogType.Event:
					return Path.Combine(GamePaths.Logs, "groups-event.txt");
				case EnumLogType.StoryEvent:
					return Path.Combine(GamePaths.Logs, "groups-storyevent.txt");
				case EnumLogType.Chat:
					return Path.Combine(GamePaths.Logs, "groups-chat.txt");
				case EnumLogType.Build:
					return Path.Combine(GamePaths.Logs, "groups-build.txt");
				case EnumLogType.VerboseDebug:
				case EnumLogType.Debug:
					return Path.Combine(GamePaths.Logs, "groups-debug.txt");
				case EnumLogType.Worldgen:
					return Path.Combine(GamePaths.Logs, "groups-worldgen.txt");
				default:
					return Path.Combine(GamePaths.Logs, "groups-main.txt");
			}
			*/
		}

		protected override void LogImpl(EnumLogType logType, string message, params object[] args)
		{
			if (!disposed)
			{
				base.LogImpl(logType, message, args);
				if (logType == EnumLogType.Error || logType == EnumLogType.Fatal)
				{
					string logFileName2 = getLogFile(EnumLogType.Event);
					LogToFile(logFileName2, logType, message, args);
				}
				if (logType == EnumLogType.Event)
				{
					string logFileName = getLogFile(EnumLogType.Notification);
					LogToFile(logFileName, logType, message, args);
				}
			}
		}

		public override bool printToConsole(EnumLogType logType)
		{
			if (logType != EnumLogType.VerboseDebug && logType != EnumLogType.StoryEvent)
			{
				return logType != EnumLogType.Audit;
			}
			return false;
		}
	}
}
