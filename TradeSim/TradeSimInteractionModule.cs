using System.Globalization;
using Discord.Interactions;
using Discord.WebSocket;
using TradeSim.BotEngine;

namespace TradeSim;

[Group("sim", "Trade Simulator Bot")]
public class TradeSimInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("start", "Start trade simulation")]
    public async Task Start(double price)
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        if (engine.State != BotState.WaitingToStart)
        {
            await RespondAsync("simulation already running. reset first.", ephemeral: true);
            return;
        }
        
        await RespondAsync($"starting simulation at {price}", ephemeral: true);
        
        await engine.Start(price, Context.Channel);
    }
    
    [SlashCommand("tick", "Proceed to next candle")]
    public async Task Tick(double price)
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        if (engine.State is BotState.RoundEnded or BotState.TakingOrders)
        {
            await RespondAsync("next tick " + price, ephemeral: true);
            await engine.Tick(price, Context.Channel);
            return;
        }

        await RespondAsync("can not tick atm", ephemeral: true);
    }
    
    [SlashCommand("scores", "Show scores")]
    public async Task Scores()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        engine.PrintScores(Context.Channel);
        
        await RespondAsync("ok", ephemeral: true);
    }
    
    [SlashCommand("pause", "Stops accepting orders from users. Useful before next tick.")]
    public async Task Pause()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.EndRound(Context.Channel);
        
        await RespondAsync("ok", ephemeral: true);
    }
    
    [SlashCommand("close-all", "Closes all open orders with closing price")]
    public async Task CloseAll(double price)
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.CloseAll(price, Context.Channel);

        await RespondAsync("ok", ephemeral: true);
    }
    
    [SlashCommand("reset", "Clears all orders & scores.")]
    public async Task Reset()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await engine.Reset(Context.Channel);
        
        await RespondAsync("simulation reset; start new session using .start <price>", ephemeral: true);
    }
    
    [SlashCommand("status", "Shows the current status of the bot.")]
    public async Task Status()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await RespondAsync(engine.State.ToString(), ephemeral: true);
    }
    
    [SlashCommand("price", "Shows the current price")]
    public async Task Price()
    {
        var engine = EngineManager.Get(Context.Channel.Id);

        await RespondAsync(engine.CurrentPrice.ToString(CultureInfo.InvariantCulture), ephemeral: true);
    }
    
    [SlashCommand("remove-order", "Remove an open order of a user")]
    public async Task RemoveOrder(SocketGuildUser user)
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        await engine.RemoveOrder(user, Context.Channel);
        
        await RespondAsync("ok", ephemeral: true);
    }
    
    [SlashCommand("set-score", "Set the score of a user")]
    public async Task SetScore(SocketGuildUser user, double points)
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        await engine.SetScore(user, points, Context.Channel);
        
        await RespondAsync("ok", ephemeral: true);
    }
    
    [SlashCommand("reset-2x", "Allow a user to do a 2x again")]
    public async Task Reset2X(SocketGuildUser user)
    {
        var engine = EngineManager.Get(Context.Channel.Id);
        await engine.Reset2x(user, Context.Channel);

        await RespondAsync("ok", ephemeral: true);
    }
    
    // [SlashCommand("test", "foo")]
    // public async Task Test([Autocomplete(typeof(OpenOrdersAutoCompleteHandler))] string user)
    // {
    //     await RespondAsync("hello " + user);
    // }
}

// public class OpenOrdersAutoCompleteHandler : AutocompleteHandler
// {
//     public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
//     {
//         var engine = EngineManager.Get(context.Channel.Id);
//
//         var list = new List<AutocompleteResult>();
//
//         foreach (var order in engine.Orders)
//         {
//             list.Add(new AutocompleteResult($"{order.User.Name} Long ({order.GetValue(engine.CurrentPrice)}){(order.Is2x ? " [2x]" : "")}", $"<@{order.User.DiscordId}>"));
//         }
//         
//         // max - 25 suggestions at a time (API limit)
//         return AutocompletionResult.FromSuccess(list);
//     }
// }