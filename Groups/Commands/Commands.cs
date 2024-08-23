using Groups.API;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;
namespace Groups.Commands
{
	public static class AddCommands
	{
		public static void run(ICoreServerAPI api)
		{
			new StandingsPlayer(api);
			new StandingsGroup(api);
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
	class StandingsGroup : Command
	{
		private const string _name = "standings";
		private const string _description = "Sets the standings of target group to value specified.";
		private static readonly string _privilege = Privilege.chat;
		private const string _subName = "group";
		public StandingsGroup(ICoreServerAPI api) : base(api, _name, _description, _privilege, _subName) { }
		public override TextCommandResult run(TextCommandCallingArgs args)
		{
			throw new System.NotImplementedException();
		}
	}

}


