namespace TradeSim.BotEngine;

public class ScoreEntry
{
    public ulong DiscordId { get; set; }
    public string Name { get; set; } = "";
    public double Points { get; set; }
    public int TradeCounter { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }

    public string WinRate
    {
        get
        {
            if (TradeCounter == 0)
            {
                return "-";
            }
            
            if (Losses == 0 && Wins > 0)
            {
                return "100 %";
            }

            return $"{(Wins / (double)Losses).ToString("0.00")}";
        }
    }
}