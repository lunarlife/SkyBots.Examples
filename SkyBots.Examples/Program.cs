using SkyBots.Api;
using SkyBots.Api.Bots;
using SkyBots.Api.Components.Entities.Bots;
using SkyBots.Api.Components.Entities.Bots.Navigate;
using SkyBots.Api.Components.Entities.Bots.TaskMachine;
using SkyBots.Api.Entities;
using SkyBots.Api.Events.Bots;
using SkyBots.Api.Events.Entities;
using SkyBots.Api.Events.Inventories;
using SkyBots.Api.Events.Inventories.SlotChange;
using SkyBots.Api.Events.Tasks;
using SkyBots.Api.Events.World;
using SkyBots.Api.Jobs.Instructions;
using SkyBots.Api.Mathematics;
using SkyBots.Api.Server;
using static SkyBots.Api.App;

namespace SkyBots.Examples;

public class Program : SkyProgram
{
    public override Token Token => "18c7ed08286";
    public override string Password => "66965";

    public override async void Init(AuthResult result)
    {
        if (result != AuthResult.Successfully) return;
        if (Island.FreeBots.Count == 0) Debug.Info("No available bots.");
        var bot = Island.FreeBots[0];
        var entity = bot.Entity;
        Debug.Info($"Try to bind bot {entity.Id}");
        var botBindingResult = await bot.Bind(new BotBindArgs
        {
            DisplayName = "Example",
        });
        ExecuteJob(TestJob());
        bot.OnInventoryOpen.AddListener(InventoryOpenHandler);
        bot.OnInventoryClose.AddListener(InventoryCloseHandler);
        bot.Inventory.OnSlotChange.AddListener(InventorySlotChangeHandler);
        bot.Transform.OnPositionChanged.AddListener(OnPositionChanged);
        bot.Transform.OnViewChanged.AddListener(OnViewChanged);
        bot.Transform.OnIsOnGroundChanged.AddListener(IsOnGroundHandler);
        bot.OnHurt.AddListener(HurtHandler);
        entity.OnDead.AddListener(DeadHandler);
        bot.OnRespawn.AddListener(RespawnHandler);
        World.OnBlockChange.AddListener(WorldBlockChangeHandler);
        if (entity.AliveStatus != AliveStatus.Alive) await bot.Respawn().WaitAsync();

        Debug.Info("Binding: " + botBindingResult);

        var world = World;
        if (!world.IsLoaded)
        {
            Debug.Info("Wait for loading world");
            await world.WaitForLoad();
            Debug.Info("World loaded");
        }

        var navigator = entity.GetComponent<Navigator>();
        var navigate = navigator.Navigate(new BotNavigateArgs
        {
            Target = new Vector3<int>(1, 101, 1),
            Sprint = true
        });
        navigate.OnCompleted.AddListener(MoveCompletedHandler);
        await navigate.WaitAsync();

        await bot.InteractBlock(new Vector3<int>(0, 101, 0)).WaitAsync();
        bot.OpenedInventory!.OnSlotChange.AddListener(args =>
        {
            Console.WriteLine($"Changed opened inventory {args.Inventory.Type} from {args.Old} to {args.New}.");
        });
    }

    private void InventorySlotChangeHandler(BotInventorySlotChangeEventArgs args)
    {
        Debug.Info($"Bot {args.Bot.Name} slot {args.Slot} changed from {args.Old} to {args.New}");
    }

    private void InventoryOpenHandler(InventoryOpenEventArgs args) =>
        Debug.Info($"Bot {args.Bot.Name} opened inventory {args.Inventory.Type}.");

    private void InventoryCloseHandler(InventoryCloseEventArgs args) =>
        Debug.Info($"Bot {args.Bot.Name} closed inventory {args.Inventory.Type}.");

    private IEnumerable<IInstruction> TestJob()
    {
        var job = new WaitMs(500);
        for (var i = 0; i < 10; i++)
        {
            yield return job;
            Debug.Info("500 ms elapsed.");
        }
    }


    private void WorldBlockChangeHandler(WorldBlockChangeEventArgs args) =>
        Debug.Info($"Block changed in {args.Old.Position} from {args.Old.Type} to {args.New.Type}.");

    private void RespawnHandler(BotRespawnEventArgs args) => Debug.Info($"Bot {args.Bot.Name} RESPAWNED");

    private void DeadHandler(EntityDeadEventArgs args)
    {
        Debug.Info($"Bot {args.Entity.Name} DEAD");
        args.Entity.GetComponent<Bot>().Respawn();
    }

    private void HurtHandler(BotHurtEventArgs args) => Debug.Info($"Bot {args.Bot.Name} hurt: {args.NewHealth}");

    private void IsOnGroundHandler(EntityIsOnGroundChangedEventArgs args) =>
        Debug.Info($"Entity is on ground changed: {args.Entity.Name}");

    private void MoveCompletedHandler(ITaskCompletedEventArgs<INavigateTask> args)
    {
        var bot = args.Task.Bot;
        Debug.Info($"Bot {bot.Name} has completed moving: {args.Task.Result}");
        Debug.Info($"Interact {bot.Transform.Position}");
        bot.InteractBlock(bot.Transform.Position);
    }

    private void OnPositionChanged(EntityPositionChangedEventArgs args) =>
        Debug.Info($"Entity {args.Entity.Name} position changed: {args.New}");

    private void OnViewChanged(EntityViewChangedEventArgs args) =>
        Debug.Info($"Entity {args.Entity.Name} view changed: {args.New}");
}