using Groups.API;
using Groups.API.Exceptions;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;
namespace Groups.Commands
{
	public static class AddCommands
	{
		public static void run(ICoreServerAPI sapi)
		{
			new StandingsPlayer(sapi);

			var gapi = sapi.ModLoader.GetModSystem<GroupsAPI>();
			sapi.ChatCommands.GetOrCreate("groups")
				.RequiresPrivilege(Privilege.chat)
				.RequiresPlayer()
				.BeginSub("set")
				.WithDescription("An Assortment of Set commands to set values within the groups system.")
				.BeginSubs("PlayerGroup")
				.EndSub()
				.BeginSub("PlayerRank")
				.EndSub()
				.BeginSub("PlayerTitle")
				.EndSub()
				.BeginSub("GroupTitles")
				.EndSub()
				.BeginSub("PlayerStanding")
				.EndSub()
				.BeginSub("PropertyValue")
				.EndSub()
				.EndSub()
				.BeginSub("get")
				.WithDescription("An Assortment of Get commands to get values within the groups system.")
				.BeginSubs("PlayerGroup")
				.WithArgs(new WordArgParser("PlayerName", false))
				.HandleWith(Handler((args) =>
				{
					String PlayerName = (args?[0] as string) ?? args.Caller.GetName();
					IServerPlayer Player = sapi.World.AllPlayers.ToList().Find(player => player.PlayerName == args[0] as string) as IServerPlayer;
					return $"{Player?.PlayerName ?? PlayerName} is apart of group id {gapi.Group.GetPlayerGroup(Player?.PlayerUID ?? PlayerName)}";
				}))
				.EndSub()
				.BeginSub("PlayerRank")
				.WithArgs(new ICommandArgumentParser[] { new IntArgParser("GroupUID", 0, Int32.MaxValue, 0, false), new WordArgParser("PlayerName", false) })
				.HandleWith(Handler((args) =>
				{
					String PlayerName = (args?[1] as string) ?? args.Caller.GetName();
					IServerPlayer Player = sapi.World.AllPlayers.ToList().Find(player => player.PlayerName == args[1] as string) as IServerPlayer;
					return $"{Player?.PlayerName ?? PlayerName} is apart of group id {gapi.Group.GetPlayerRank(Player?.PlayerUID ?? PlayerName, (int)args?[0])}";
				}))
				.EndSub()
				.BeginSub("PlayerTitle")
				.WithArgs(new ICommandArgumentParser[] { new IntArgParser("GroupUID", 0, Int32.MaxValue, 0, false), new WordArgParser("PlayerName", false) })
				.HandleWith(Handler((args) =>
				{
					String PlayerName = (args?[1] as string) ?? args.Caller.GetName();
					IServerPlayer Player = sapi.World.AllPlayers.ToList().Find(player => player.PlayerName == args[1] as string) as IServerPlayer;
					return $"{Player?.PlayerName ?? PlayerName} is apart of group id {gapi.Group.GetPlayerTitle(Player?.PlayerUID ?? PlayerName, (int)args?[0])}";
				}))
				.EndSub()
				.BeginSub("PlayerStanding")
				.WithArgs(new ICommandArgumentParser[] { new IntArgParser("GroupUID", 0, Int32.MaxValue, 0, true), new WordArgParser("PlayerName", false) })
				.HandleWith(Handler((args) =>
				{
					String PlayerName = (args?[1] as string) ?? args.Caller.GetName();
					IServerPlayer Player = sapi.World.AllPlayers.ToList().Find(player => player.PlayerName == args[1] as string) as IServerPlayer;
					return $"{Player?.PlayerName ?? PlayerName} is apart of group id {gapi.Group.GetPlayerStanding((int)args[0], Player?.PlayerUID ?? PlayerName)}";
				}))
				.EndSub()
				.BeginSub("GroupTitles")
				.WithArgs(new IntArgParser("Group UID", 0, Int32.MaxValue, 0, true))
				.HandleWith(Handler((args) => { return $"[{String.Join(", ", gapi.Group.GetGroupTitles((int)args[0]))}]"; }))
				.EndSub()
				.BeginSub("PropertyValue")
				.WithArgs(new ICommandArgumentParser[] { new IntArgParser("Proptery UID", 0, Int32.MaxValue, 0, true), new IntArgParser("Group UID", 0, Int32.MaxValue, 0, true) })
				.HandleWith(Handler((args) => { return $"[{String.Join(", ", gapi.Group.GetPropertyValue((int)args[0], (int)args[1]).ToString())}]"; }))
				.EndSub()
				.EndSub()
				.BeginSub("dump")
				.WithDescription("Dumps the value of the selected group to the chat box.")
#if DEBUG
				.WithArgs(new IntArgParser("Group UID", 0, Int32.MaxValue, -1, false))
				.HandleWith(Handler((args) =>
				{
					if (args[0] is null or <= -1) return gapi.Group.DumpGroups();
#else
				.WithArgs(new IntArgParser("Group UID", 0, Int32.MaxValue, -1, true))
				.HandleWith(Handler((args) =>
				{
#endif
					return gapi.Group.GetGroupSettings((int)args[0]).ToString();
				}));
		}
		public delegate string HandlerDelegate(TextCommandCallingArgs args);
		public static OnCommandDelegate Handler(HandlerDelegate hd)
		{
			return (args) =>
			{
				try
				{
					return TextCommandResult.Success(hd(args));
				}
				catch (InvalidGroupUID ex) { return TextCommandResult.Error("Invalid Group UID: ", ex.ToString()); }
				catch (InvalidPlayerUID ex) { return TextCommandResult.Error("Invalid Player UID: ", ex.ToString()); }
				catch (InvalidPropertyUID ex) { return TextCommandResult.Error("Invalid Property UID: ", ex.ToString()); }
				catch (ArgumentException ex) { return TextCommandResult.Error("Invalid Argument: ", ex.ToString()); }
				catch (InvalidProperty ex) { return TextCommandResult.Error("Invalid Prooprtery: Should not be possible...", ex.ToString()); }
				catch (LoadIncompleteException ex) { return TextCommandResult.Error("LoadIncomplete: Should not be possible...", ex.ToString()); }
				catch (Exception ex) { return TextCommandResult.Error("Unknon Error: ", ex.ToString()); }
			};
		}
	}
	internal abstract class Command
	{
		public Command(ICoreServerAPI api, string _name, string _description, string _privilege, string _subName = null, ICommandArgumentParser[] _args = null)
		{
			IChatCommand command = api.ChatCommands.GetOrCreate(_name)
			  .RequiresPrivilege(_privilege)
			  .RequiresPlayer();
			if (_subName != null)
			{
				if (_args != null)

				{
					command.BeginSub(_subName)
						.WithArgs(_args)
						.WithDescription(_description)
						.HandleWith(run);
				}
				else
				{
					command.BeginSub(_subName)
						.WithDescription(_description)
						.HandleWith(run);
				}

			}
			else
			{
				if (_args != null)
				{
					command.WithArgs(_args);
				}
				command.WithDescription(_description)
					.HandleWith(run);
			}

		}
		abstract public TextCommandResult run(TextCommandCallingArgs args);
	}
	class StandingsPlayer : Command
	{
		ICoreServerAPI sapi;
		private const string _name = "standings";
		private const string _description = "Sets the standings of target player to value specified.";
		private static readonly string _privilege = Privilege.chat;
		private const string _subName = "player";
		private static readonly ICommandArgumentParser[] _args = { new WordArgParser("PlayerName", true), new IntArgParser("newStandings", -100, 100, 0, true) { } };
		public StandingsPlayer(ICoreServerAPI api) : base(api, _name, _description, _privilege, _subName, _args) { sapi = api; }

		public override TextCommandResult run(TextCommandCallingArgs args)
		{
			string PlayerName;
			IServerPlayer CallingPlayer;
			IServerPlayer StandingsPlayer;
			sbyte standing;
			try
			{
				PlayerName = args[0] as string;
				CallingPlayer = args.Caller.Player as IServerPlayer;
				StandingsPlayer = sapi.World.AllPlayers.ToList().Find(player => player.PlayerName == PlayerName) as IServerPlayer;
				standing = Convert.ToSByte(args[1]);
			}
			catch (Exception e) { return TextCommandResult.Error("Unknown Error: Exception Type: " + e.GetType().Name); }
			if (StandingsPlayer == null) { return TextCommandResult.Error($"Unknow Player: Was uanble to find player by player name \"{PlayerName}\""); }
			try
			{
				sapi.ModLoader.GetModSystem<GroupsAPI>().Player.SetStanding(CallingPlayer, StandingsPlayer, standing);

				return TextCommandResult.Success($"Updated {PlayerName}'s standings to {standing}!");
			}
			catch (Exception e) { return TextCommandResult.Error("Unknown Error while saving. Exception Type: " + e.GetType().Name); }
		}
	}
}



