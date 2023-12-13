using SkyBots.Api;
using SkyBots.Api.Bots;
using SkyBots.Api.Components.Entities.Bots;
using SkyBots.Api.Components.Entities.Bots.Internal.Navigate;
using SkyBots.Api.Events.Bots.Navigate;
using SkyBots.Api.Mathematics;
using SkyBots.Api.Server;

namespace SkyBots.Examples;

public class Program : SkyProgram
{
    public override Token Token => "18c5fc0d72b";
    public override string Password => "87367";

    public override async void Init(AuthResult result)
    {
        if (result != AuthResult.Successfully) return;
        if (Island.FreeBots.Count == 0) Console.WriteLine("No available bots.");
        var entity = Island.FreeBots[0];
        var bot = entity.GetComponent<BotComponent>();
        var botBindingResult = await bot.Bind(new BotBindArgs
        {
            DisplayName = "Example",
        });
        App.Debug.Info("Binding: " + botBindingResult);
        var plugin = entity.Transform;
        plugin.OnMoved.AddListener(OnPositionChanged);
        var target = new Vector3<float>(5f, 101, 5f);
        App.Debug.Info($"Initial bot moving to {target};");
        var task = entity.GetComponent<Navigator>().Move(new MoveArgs
        {
            Sprint = false,
            Target = target
        });
        task.OnComplete.AddListener(MoveCompleteHandler);
        var world = App.World;
        if (!world.IsLoaded)
        {
            App.Debug.Info("Wait for loading world");
            await world.WaitForLoad();
            App.Debug.Info("World loaded");
        }

        App.Debug.Info(world.GetBlockAt(0, 100, 0).Type);
    }

    private void MoveCompleteHandler(BotMoveCompletedEventArgs args)
    {
        App.Debug.Info($"Bot {args.Bot.Name} has completed moving : " + args.Result);
    }

    private void OnPositionChanged(EntityPositionChangedEventArgs args)
    {
        App.Debug.Info($"bot {args.Component.Entity.Name} moved to " + args.New);
    }
}