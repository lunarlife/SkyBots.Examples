using SkyBots.Api;
using SkyBots.Api.Bots;
using SkyBots.Api.Components.Entities.Bots;
using SkyBots.Api.Components.Entities.Bots.Internal.Navigate;
using SkyBots.Api.Events.Bots.Navigate;
using SkyBots.Api.Events.Entities;
using SkyBots.Api.Mathematics;
using SkyBots.Api.Server;

namespace SkyBots.Examples;

public class Program : SkyProgram
{
    public override Token Token => "18c65790553";
    public override string Password => "25926";

    public override async void Init(AuthResult result)
    {
        if (result != AuthResult.Successfully) return;
        if (Island.FreeBots.Count == 0) App.Debug.Info("No available bots.");
        var entity = Island.FreeBots[0];
        var bot = entity.GetComponent<BotComponent>();
        App.Debug.Info($"Try to bind bot {entity.Id}");
        var botBindingResult = await bot.Bind(new BotBindArgs
        {
            DisplayName = "Example",
        });
        if (!entity.IsAlive) bot.Respawn();
        App.Debug.Info("Binding: " + botBindingResult); 
        var transform = entity.Transform;
        transform.OnPositionChanged.AddListener(OnPositionChanged);
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
        var bot = args.Bot;
        App.Debug.Info($"Bot {bot.Name} has completed moving : " + args.Result);
        var target = (Vector3<int>)bot.Transform.Position;
        var block = target + Vector3<int>.DOWN;
        Console.WriteLine($"Bot position: {target}; target block: {block}");
        bot.BreakBlock(block);
    }

    private void OnPositionChanged(TransformChangedEventArgs args)
    {
        //App.Debug.Info($"Bot {args.Component.Entity.Name} moved to " + args.Component);
    }
}