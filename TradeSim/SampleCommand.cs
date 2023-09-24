using Discord.Commands;
using Discord.WebSocket;
using TradeSim.BotEngine;

namespace TradeSim;

public class HelloModule : ModuleBase<SocketCommandContext>
{
    [Command("hello")]
    public async Task Hello()
    {
        await ReplyAsync($"Hello, {Context.User.Mention}!");
    }

    [Command("start")]
    public async Task Start([Remainder] double price)
    {
        Console.WriteLine("starting " + Context.Channel.Id);
        
        var engine = EngineManager.Get(Context.Channel.Id);

        if (engine.State != BotState.WaitingToStart)
        {
            await ReplyAsync("already started!");
            return;
        }
        
        engine.Start(price, Context);
    }
    
    
    [Command("tick")]
    public async Task Tick([Remainder] double price)
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        if (engine.State is BotState.RoundEnded or BotState.TakingOrders)
        {
            await engine.Tick(price, Context);
            return;
        }
        
        await ReplyAsync("can not tick atm");
    }

    [Command("scores")]
    public async Task Scores()
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        
        engine.PrintScores(Context);
    }
    
    [Command("pause")]
    public async Task Pause()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.EndRound(Context);
    }

    [Command("close")]
    public async Task CloseAll([Remainder] double price)
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.CloseAll(price, Context);
    }
    
    [Command("reset")]
    public async Task Reset()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.Reset(Context);
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
        await engine.RemoveOrder(user, Context);
    }

    [Command("set-score")]
    public async Task SetScore(SocketGuildUser user, [Remainder]double points)
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        await engine.SetScore(user, points, Context);
    }
    
    [Command("reset-2x")]
    public async Task Reset2x(SocketGuildUser user)
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        await engine.Reset2x(user, Context);
    }
}