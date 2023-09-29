namespace TradeSim.BotEngine;

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