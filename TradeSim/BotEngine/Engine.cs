using System.Text;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

namespace TradeSim.BotEngine;

public enum OrderType
{
    Long,
    Short,
}

public class User
{
    public string Id { get; set; } = "";
    public ulong DiscordId { get; set; }
    public string Name { get; set; } = "";
}

public class Order
{
    public User User { get; set; }
    public OrderType Type { get; set; }
    public bool Is2x { get; set; }
    public double Price { get; set; }

    public double GetValue(double currentPrice)
    {
        if (Type == OrderType.Long)
        {
            return (currentPrice - Price) * (Is2x ? 2 : 1);
        }

        if (Type == OrderType.Short)
        {
            return (Price - currentPrice) * (Is2x ? 2 : 1);
        }

        return 0;
    }
}

public class Scores
{
    public List<ScoreEntry> ScoreEntries { get; set; } = new();

    public void UpdateScore(ulong discordId, string name, double points)
    {
        var entry = ScoreEntries.FirstOrDefault(p => p.DiscordId == discordId);

        if (entry == null)
        {
            entry = new ScoreEntry
            {
                DiscordId = discordId,
                Name = name,
            };

            ScoreEntries.Add(entry);
        }

        entry.Points += points;
    }

    public void Reset()
    {
        ScoreEntries.Clear();
    }

    public void SetScore(SocketGuildUser user, double points)
    {
        var entry = ScoreEntries.FirstOrDefault(p => p.DiscordId == user.Id);

        if (entry == null)
        {
            entry = new ScoreEntry
            {
                DiscordId = user.Id,
                Name = user.Username,
            };

            ScoreEntries.Add(entry);
        }

        entry.Points = points;
    }
}

public class ScoreEntry
{
    public ulong DiscordId { get; set; }
    public string Name { get; set; } = "";
    public double Points { get; set; }
}

public class Engine
{
    private RestUserMessage? takingOrdersMessage;
    public BotState State { get; private set; } = BotState.WaitingToStart;

    public double CurrentPrice { get; set; }

    public List<Order> Orders { get; set; } = new();
    public Scores Scores { get; set; } = new();

    public List<ulong> Doubled { get; set; } = new();    

    public async Task Start(double price, ISocketMessageChannel channel)
    {
        State = BotState.TakingOrders;
        CurrentPrice = price;

        await CreateTakingOrdersMessage(channel);
    }

    public async Task Reset(ISocketMessageChannel channel)
    {
        Orders.Clear();
        Scores.Reset();
        Doubled.Clear();

        State = BotState.WaitingToStart;

        await channel.SendMessageAsync("simulation reset; start using .start <price>");
    }

    public async Task ShowOrders(SocketCommandContext context)
    {
        if (Orders.Count == 0)
        {
            await context.Channel.SendMessageAsync("there are no orders");
            return;
        }

        var sb = new StringBuilder();

        CreateOpenOrderList(sb);

        await context.Channel.SendMessageAsync(sb.ToString());
    }

    public async Task CloseAll(double price, ISocketMessageChannel channel)
    {
        CurrentPrice = price;

        foreach (var order in Orders)
        {
            var points = order.GetValue(CurrentPrice);

            Scores.UpdateScore(order.User.DiscordId, order.User.Name, points);

            await channel.SendMessageAsync(
                $"closing {order.Type} of <@{order.User.DiscordId}> for {points:+#.##;-#.##;0}");
        }

        Orders.Clear();
    }

    public async Task EndRound(ISocketMessageChannel channel)
    {
        State = BotState.RoundEnded;
        await channel.SendMessageAsync("paused, waiting for next tick");
    }

    public async Task Tick(double price, ISocketMessageChannel channel)
    {
        State = BotState.RoundEnded;

        CurrentPrice = price;

        await CreateTakingOrdersMessage(channel);
    }

    private async Task CreateTakingOrdersMessage(ISocketMessageChannel channel)
    {
        takingOrdersMessage = await channel.SendMessageAsync(":arrow_right:  **Taking orders...**");

        State = BotState.TakingOrders;

        await UpdateTakingOrdersMessage();

        await takingOrdersMessage.AddReactionAsync(Settings.Long);
        await takingOrdersMessage.AddReactionAsync(Settings.Short);
        await takingOrdersMessage.AddReactionAsync(Settings.Close);
        await takingOrdersMessage.AddReactionAsync(Settings.Reverse);
        await takingOrdersMessage.AddReactionAsync(Settings.DoubleDown);
    }

    public async Task Long(ulong reactionUserId, IUser userValue, IMessageChannel messageChannel)
    {
        if (State != BotState.TakingOrders)
        {
            await messageChannel.SendMessageAsync($"<@{reactionUserId}> not taking any orders atm");
            return;
        }

        var existingOrder = Orders.FirstOrDefault(p => p.User.DiscordId == reactionUserId);

        if (existingOrder != null)
        {
            await messageChannel.SendMessageAsync($"<@{reactionUserId}> has already an open order an can not go long");
            return;
        }

        Orders.Add(new Order
        {
            Is2x = false,
            Price = CurrentPrice,
            Type = OrderType.Long,
            User = new User
            {
                DiscordId = reactionUserId,
                Name = userValue?.ToString() ?? "",
            }
        });

        await UpdateTakingOrdersMessage();
    }

    public async Task Short(ulong reactionUserId, IUser userValue, IMessageChannel messageChannel)
    {
        if (State != BotState.TakingOrders)
        {
            await messageChannel.SendMessageAsync($"<@{reactionUserId}> not taking any orders atm");
            return;
        }

        var existingOrder = Orders.FirstOrDefault(p => p.User.DiscordId == reactionUserId);

        if (existingOrder != null)
        {
            await messageChannel.SendMessageAsync($"<@{reactionUserId}> has already an open order an can not go short");
            return;
        }

        Orders.Add(new Order
        {
            Is2x = false,
            Price = CurrentPrice,
            Type = OrderType.Short,
            User = new User
            {
                DiscordId = reactionUserId,
                Name = userValue?.ToString() ?? "",
            }
        });

        await UpdateTakingOrdersMessage();
    }

    public async Task Close(ulong reactionUserId, IUser userValue, IMessageChannel messageChannel)
    {
        if (State != BotState.TakingOrders)
        {
            await messageChannel.SendMessageAsync($"<@{reactionUserId}> not taking any orders atm");
            return;
        }
        
        var order = Orders.FirstOrDefault(p => p.User.DiscordId == reactionUserId);

        if (order == null)
        {
            await messageChannel.SendMessageAsync($"{userValue} no order found to close");
            return;
        }

        await CloseOrder(reactionUserId, userValue, messageChannel, order);

        await UpdateTakingOrdersMessage();
    }

    private async Task CloseOrder(ulong reactionUserId, IUser userValue, IMessageChannel messageChannel, Order order,
        bool reverse = false)
    {
        var points = order.GetValue(CurrentPrice);

        Scores.UpdateScore(reactionUserId, userValue.Username, points);

        await messageChannel.SendMessageAsync(
            $"<@{reactionUserId}> closed {order.Type} position for **{points:+#;-#;0}**{(reverse ? " (:repeat:)" : "")}");

        Orders.Remove(order);
    }

    public void PrintScores(ISocketMessageChannel channel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Scores");
        sb.AppendLine("------");

        var i = 1;

        foreach (var entry in Scores.ScoreEntries.OrderByDescending(o => o.Points))
        {
            var prefix = "";

            if (i == 1)
            {
                prefix = ":first_place: ";
            }
            else if (i == 2)
            {
                prefix = ":second_place: ";
            }
            else if (i == 3)
            {
                prefix = ":third_place: ";
            }

            if (entry.Points < 0)
            {
                prefix = ":red_square: ";
            }

            sb.AppendLine($"{prefix}**{entry.Points}** <@{entry.DiscordId}>");
            ++i;
        }

        sb.AppendLine();

        if (Orders.Count > 0)
        {
            CreateOpenOrderList(sb);
        }

        channel.SendMessageAsync(sb.ToString());
    }

    private async Task UpdateTakingOrdersMessage()
    {
        var sb = new StringBuilder();
        sb.AppendLine(":arrow_right:  **Taking orders...**");
        sb.AppendLine();

        if (Orders.Count > 0)
        {
            CreateOpenOrderList(sb);
        }

        sb.AppendLine("_click reactions to take an action:_");

        await takingOrdersMessage!.ModifyAsync(props => props.Content = sb.ToString());
    }

    private void CreateOpenOrderList(StringBuilder sb)
    {
        sb.AppendLine("open orders:");

        foreach (var order in Orders.OrderBy(o => o.User.Name))
        {
            var icon = "";

            if (order.Type == OrderType.Long)
            {
                icon = ":green_circle:";
            }
            else if (order.Type == OrderType.Short)
            {
                icon = ":red_circle:";
            }

            sb.AppendLine(
                $"{icon}   <@{order.User.DiscordId}>  is {order.Type}{(order.Is2x ? " **2x**" : "")} ({order.GetValue(CurrentPrice).ToString("+#0.##;-#0.##;0.##")})");
        }

        sb.AppendLine();
    }

    public async Task RemoveOrder(SocketGuildUser user, ISocketMessageChannel channel)
    {
        var order = Orders.FirstOrDefault(p => p.User.DiscordId == user.Id);

        if (order == null)
        {
            await channel.SendMessageAsync("no order found for user");
            return;
        }

        Orders.Remove(order);

        await channel.SendMessageAsync("order removed");
    }
    
    public async Task Reset2x(SocketGuildUser user, ISocketMessageChannel channel)
    {
        if (Doubled.Contains(user.Id))
        {
            Doubled.Remove(user.Id);
            await channel.SendMessageAsync($"<@{user.Id}> you can 2x again. gl.");
            return;
        }
    }

    public async Task SetScore(SocketGuildUser user, double points, ISocketMessageChannel channel)
    {
        Scores.SetScore(user, points);

        await channel.SendMessageAsync("score set");
    }

    public async Task Reverse(ulong reactionUserId, IUser userValue, IMessageChannel channel)
    {
        if (State != BotState.TakingOrders)
        {
            await channel.SendMessageAsync($"<@{reactionUserId}> not taking any orders atm");
            return;
        }
        
        var order = Orders.FirstOrDefault(p => p.User.DiscordId == reactionUserId);

        if (order == null)
        {
            await channel.SendMessageAsync($"no order for <@{reactionUserId}> found to reverse");
            return;
        }

        await CloseOrder(reactionUserId, userValue, channel, order, true);

        if (order.Type == OrderType.Long)
        {
            await Short(reactionUserId, userValue, channel);
        }
        else if (order.Type == OrderType.Short)
        {
            await Long(reactionUserId, userValue, channel);
        }
    }

    public async Task DoubleDown(ulong reactionUserId, IUser userValue, IMessageChannel channel)
    {
        if (State != BotState.TakingOrders)
        {
            await channel.SendMessageAsync($"<@{reactionUserId}> not taking any orders atm");
            return;
        }

        if (Doubled.Contains(reactionUserId))
        {
            await channel.SendMessageAsync($"<@{reactionUserId}> can't 2x again :exclamation:");
            return;
        }

        var order = Orders.FirstOrDefault(p => p.User.DiscordId == reactionUserId);

        if (order == null)
        {
            await channel.SendMessageAsync($"<@{reactionUserId}> no order found to 2x");
            return;
        }

        var points = order.GetValue(CurrentPrice);

        if (points != 0)
        {
            Scores.UpdateScore(reactionUserId, userValue.Username, points);
            
            await channel.SendMessageAsync(
                $"<@{reactionUserId}> closed existing {order.Type} position for **{points:+#0.##;-#0.##;0.##}** & opened **2x** {order.Type} position");
        }
        else
        {
            await channel.SendMessageAsync(
                $"<@{reactionUserId}> upgraded {order.Type} position **2x**");
        }

        order.Price = CurrentPrice;
        order.Is2x = true;

        Doubled.Add(reactionUserId);

        await UpdateTakingOrdersMessage();
    }
}

public enum BotState
{
    WaitingToStart,
    TakingOrders,
    RoundEnded,
}