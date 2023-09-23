using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

class Program
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private IServiceProvider _services;

    static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

    public async Task RunBotAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents =  GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.MessageContent
        });
        
        _commands = new CommandService();
        
        _client.Log += LogAsync;
        _client.Ready += OnReadyAsync;

        await RegisterCommandsAsync();

        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("BOT_TOKEN"));
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    public async Task RegisterCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        _client.MessageUpdated += _clientOnMessageUpdated;
        _client.ReactionAdded += HandleReactionAdded;
        _client.ReactionRemoved += HandleReactionRemoved;

        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        Console.WriteLine("reaction removed");

        return Task.CompletedTask;
    }

    private Task HandleReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        Console.WriteLine("reaction added");

        return Task.CompletedTask;
    }

    private Task _clientOnMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
    {
        Console.WriteLine("message updated");

        return Task.CompletedTask;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        
        return Task.CompletedTask;
    }

    private async Task OnReadyAsync()
    {
        Console.WriteLine($"Logged in as {_client.CurrentUser.Username}");
        
       
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        var context = new SocketCommandContext(_client, message);
        
        Console.WriteLine("message: " + message?.Content);
        
        if (message.Author.IsBot) return;

        int argPos = 0;
        
        if (message.HasStringPrefix(".", ref argPos))
        {
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
        }
        else if (message.Content == "cd")
        {
            if (context.Channel != null)
            {
                // Set the end time for the countdown (e.g., 5 minutes from now)
                var endTime = DateTime.Now.AddMinutes(5);
                
                string GetCountdownString()
                {
                    TimeSpan remainingTime = endTime - DateTime.Now;
                   
                    if (remainingTime.TotalSeconds <= 0)
                    {
                        return "Countdown expired!";
                    }

                    return $"Time remaining: {remainingTime.ToString(@"mm\:ss")}";
                }
            
                // Send the initial countdown message
                var countdownMessage = await context.Channel.SendMessageAsync(GetCountdownString());

                // Schedule a task to update the countdown every second
                
                var timer = new System.Threading.Timer((object state) =>
                {
                    if (countdownMessage != null)
                    {
                        countdownMessage.ModifyAsync(props => props.Content = GetCountdownString());
                    }
                }, null, 0, 1000);
            }
        }
        else
        {
            var message2 = await context.Channel.SendMessageAsync("yolo");
            // LONG 🟢 / SHORT🔴  / CLOSE ❌/ REVERSE 🔁
            
            await message2.AddReactionAsync(new Emoji("🟢"));
            await message2.AddReactionAsync(new Emoji("🔴"));
            await message2.AddReactionAsync(new Emoji("❌"));
            await message2.AddReactionAsync(new Emoji("🔁"));
        }
    }
}