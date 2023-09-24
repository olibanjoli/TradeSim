using Discord.Commands;
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
        
        engine.Start(price);
        
        
    }
}