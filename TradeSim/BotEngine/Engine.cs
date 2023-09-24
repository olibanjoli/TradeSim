namespace TradeSim.BotEngine;

public enum OrderType
{
    Long,
    Short,
}

public class User
{
    public string Id { get; set; } = "";
    public string DiscordId { get; set; } = "";
    public string Name { get; set; } = "";
}

public class Order
{
    public User User { get; set; }
    public OrderType Type { get; set; }
    public bool Is2x { get; set; }
    public double Price { get; set; }
}

public class Engine
{
    public BotState State { get; private set; }
    
    public double CurrentPrice { get; set; }

    public List<Order> Orders { get; set; } = new();

    public Engine()
    {
        State = BotState.WaitingToStart;
    }
    
    public void Start(double price)
    {
        Console.WriteLine("starting " + price);
    }

    public void EndRound(double price)
    {
        
    }
    
    public void AddOrder()
    {
        
    }

    public void RemoveOrder()
    {
    }

    public void Register()
    {
        
    }
    
    
}

public enum BotState
{
    WaitingToStart,
    TakingOrders,
    RoundEnded,
    Finished,
}