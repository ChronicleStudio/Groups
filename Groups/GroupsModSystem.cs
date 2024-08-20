using Groups.Commands;
using Groups.GUI;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace Groups
{
    public class GroupsModSystem : ModSystem
    {

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            api.Logger.Notification("Hello from template mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            api.Logger.Notification("Hello from Groups mod server side: " + Lang.Get("groups:hello"));
            AddCommands.run(api);
            
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            new PlayersGUI(api);
            api.Logger.Notification("Hello from Groups mod client side: " + Lang.Get("groups:hello"));
        }

    }
}
