using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using TradeSim.BotEngine;

namespace TradeSim;

public class TextCommandModule : ModuleBase<SocketCommandContext>
{
    [Command("start")]
    public async Task Start(double price)
    {
        Console.WriteLine("starting " + Context.Channel.Id);

        var engine = EngineManager.Get(Context.Channel.Id);

        if (engine.State != BotState.WaitingToStart)
        {
            await ReplyAsync("simulation already running. reset first.");
            return;
        }

        await engine.Start(price, Context.Channel);
    }

    [Command("tick")]
    public async Task Tick([Remainder] double price)
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        if (engine.State is BotState.RoundEnded or BotState.TakingOrders)
        {
            await engine.Tick(price, Context.Channel);
            return;
        }

        await ReplyAsync("can not tick atm");
    }

    [Command("scores")]
    public async Task Scores()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        engine.PrintScores(Context.Channel);
    }

    [Command("pause")]
    public async Task Pause()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.EndRound(Context.Channel);
    }

    [Command("close")]
    public async Task CloseAll([Remainder] double price)
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.CloseAll(price, Context.Channel);
    }

    [Command("reset")]
    public async Task Reset()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.Reset(Context.Channel);
    }

    [Command("orders")]
    public async Task Orders()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.ShowOrders(Context);
    }

    [Command("remove-order")]
    public async Task RemoveOrder(SocketGuildUser user)
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        await engine.RemoveOrder(user, Context.Channel);
    }

    [Command("set-score")]
    public async Task SetScore(SocketGuildUser user, [Remainder] double points)
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        await engine.SetScore(user, points, Context.Channel);
    }

    [Command("reset-2x")]
    public async Task Reset2x(SocketGuildUser user)
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        await engine.Reset2x(user, Context.Channel);
    }
}