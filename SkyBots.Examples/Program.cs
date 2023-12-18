﻿using SkyBots.Api;
using SkyBots.Api.Bots;
using SkyBots.Api.Components.Entities.Bots;
using SkyBots.Api.Components.Entities.Bots.Navigate;
using SkyBots.Api.Entities;
using SkyBots.Api.Events.Bots;
using SkyBots.Api.Events.Bots.Navigate;
using SkyBots.Api.Events.Entities;
using SkyBots.Api.Events.World;
using SkyBots.Api.Jobs.Instructions;
using SkyBots.Api.Mathematics;
using SkyBots.Api.Server;
using static SkyBots.Api.App;

namespace SkyBots.Examples;
 
public class Program : SkyProgram
{
    public override Token Token => "18c79c3840e";
    public override string Password => "53848";

    public override async void Init(AuthResult result)
    {
        if (result != AuthResult.Successfully) return;
        if (Island.FreeBots.Count == 0) Debug.Info("No available bots.");
        var entity = Island.FreeBots[0];
        var bot = entity.GetComponent<BotComponent>();
        Debug.Info($"Try to bind bot {entity.Id}");
        var botBindingResult = await bot.Bind(new BotBindArgs
        {
            DisplayName = "Example",
        });
        ExecuteJob(TestJob());
        bot.OnPositionChanged.AddListener(OnPositionChanged);
        bot.OnViewChanged.AddListener(OnViewChanged);
        bot.OnIsOnGroundChanged.AddListener(IsOnGroundHandler);
        bot.OnHurt.AddListener(HurtHandler);
        entity.OnDead.AddListener(DeadHandler);
        bot.OnRespawn.AddListener(RespawnHandler);
        World.OnBlockChange.AddListener(WorldBlockChangeHandler);
        if (entity.AliveStatus != AliveStatus.Alive) await bot.Respawn().WaitAsync();

        Debug.Info("Binding: " + botBindingResult);

        var target = new Vector3<float>(5f, 101, 5f);
        Debug.Info($"Initial bot moving to {target};");
        var task = entity.GetComponent<Navigator>().Move(new MoveArgs
        {
            Sprint = false,
            Target = target
        });
        task.OnCompleted.AddListener(MoveCompleteHandler);
        var world = World;
        if (!world.IsLoaded)
        {
            Debug.Info("Wait for loading world");
            await world.WaitForLoad();
            Debug.Info("World loaded");
        }

        Debug.Info(world.GetBlockAt(0, 100, 0).Type);
    }

    private IEnumerable<IInstruction> TestJob()
    {
        var job = new WaitMs(500);
        for (var i = 0; i < 10; i++)
        {
            yield return job;
            Debug.Info("500 ms elapsed.");
        }
    }


    private void WorldBlockChangeHandler(WorldBlockChangeEventArgs args) => Debug.Info($"Block changed in {args.Old.Position} from {args.Old.Type} to {args.New.Type}.");

    private void RespawnHandler(BotRespawnEventArgs args)=> Debug.Info($"Bot {args.Bot.Name} RESPAWNED");

    private void DeadHandler(EntityDeadEventArgs args)
    {
        Debug.Info($"Bot {args.Entity.Name} DEAD");
        args.Entity.GetComponent<BotComponent>().Respawn();
    }

    private void HurtHandler(BotHurtEventArgs args) => Debug.Info($"Bot {args.Bot.Name} hurt: {args.NewHealth}");

    private void IsOnGroundHandler(EntityIsOnGroundChangedEventArgs args) =>
        Debug.Info($"Entity is on ground changed: {args.Entity.Name}");

    private void MoveCompleteHandler(TaskBotMoveCompletedEventArgs args) =>
        Debug.Info($"Bot {args.Bot.Name} has completed moving: {args.Result}");

    private void OnPositionChanged(EntityPositionChangedEventArgs args) =>
        Debug.Info($"Entity {args.Entity.Name} position changed: {args.New}");

    private void OnViewChanged(EntityViewChangedEventArgs args) =>
        Debug.Info($"Entity {args.Entity.Name} view changed: {args.New}");
}