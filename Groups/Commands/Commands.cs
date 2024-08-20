
using Vintagestory.API.Common;
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
        public Command(ICoreServerAPI api, string _name, string _description, string _privilege)
        {
            api.ChatCommands.Create(_name)
            .WithDescription(_description)
            .RequiresPrivilege(_privilege) 
            .RequiresPlayer()
            .HandleWith((args) => { return run(args); });
        }
        abstract public TextCommandResult run(TextCommandCallingArgs args);
    }
    class StandingsPlayer : Command
    {
        private const string _name = "standings.player";
        private const string _description = "Sets the standings of target player to value specified.";
        private static readonly string _privilege = Privilege.chat;
        public StandingsPlayer(ICoreServerAPI api) : base(api, _name, _description, _privilege) { }
        public override TextCommandResult run(TextCommandCallingArgs args)
        {
            throw new System.NotImplementedException();
        }
    }
    class StandingsGroup : Command
    {
        private const string _name = "standings.group";
        private const string _description = "Sets the standings of target group to value specified.";
        private static readonly string _privilege = Privilege.chat;
        public StandingsGroup(ICoreServerAPI api) : base(api, _name, _description, _privilege) { }
        public override TextCommandResult run(TextCommandCallingArgs args)
        {
            throw new System.NotImplementedException();
        }
    }

}


