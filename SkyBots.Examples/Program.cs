using SkyBots.Api;
using SkyBots.Api.Bots;
using SkyBots.Api.Components.Entities.Bots;
using SkyBots.Api.Components.Entities.Bots.Navigate;
using SkyBots.Api.Components.Entities.Bots.Respawn;
using SkyBots.Api.Entities;
using SkyBots.Api.Events.Bots;
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
        bot.OnPositionChanged.AddListener(OnPositionChanged);
        bot.OnViewChanged.AddListener(OnViewChanged);
        bot.OnIsOnGroundChanged.AddListener(IsOnGroundHandler);
        bot.OnHurt.AddListener(HurtHandler);
        entity.OnDead.AddListener(DeadHandler);
        bot.OnRespawn.AddListener(RespawnHandler);
        if (entity.AliveStatus != AliveStatus.Alive) await bot.Respawn().WaitAsync();

        App.Debug.Info("Binding: " + botBindingResult);

        var target = new Vector3<float>(5f, 101, 5f);
        App.Debug.Info($"Initial bot moving to {target};");
        var task = entity.GetComponent<Navigator>().Move(new MoveArgs
        {
            Sprint = false,
            Target = target
        });
        task.OnCompleted.AddListener(MoveCompleteHandler);
        var world = App.World;
        if (!world.IsLoaded)
        {
            App.Debug.Info("Wait for loading world");
            await world.WaitForLoad();
            App.Debug.Info("World loaded");
        }

        App.Debug.Info(world.GetBlockAt(0, 100, 0).Type);
    }

    private void RespawnHandler(BotRespawnEventArgs args)=> App.Debug.Info($"Bot {args.Bot.Name} RESPAWNED");

    private void DeadHandler(EntityDeadEventArgs args)
    {
        App.Debug.Info($"Bot {args.Entity.Name} DEAD");
        args.Entity.GetComponent<BotComponent>().Respawn();
    }

    private void HurtHandler(BotHurtEventArgs args) => App.Debug.Info($"Bot {args.Bot.Name} hurt: {args.NewHealth}");

    private void IsOnGroundHandler(EntityIsOnGroundChangedEventArgs args) =>
        App.Debug.Info($"Entity is on ground changed: {args.Entity.Name}");

    private void MoveCompleteHandler(TaskBotMoveCompletedEventArgs args) =>
        App.Debug.Info($"Bot {args.Bot.Name} has completed moving: {args.Result}");

    private void OnPositionChanged(EntityPositionChangedEventArgs args) =>
        App.Debug.Info($"Entity {args.Entity.Name} position changed: {args.New}");

    private void OnViewChanged(EntityViewChangedEventArgs args) =>
        App.Debug.Info($"Entity {args.Entity.Name} view changed: {args.New}");
}