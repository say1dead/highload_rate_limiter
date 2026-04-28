using System.Globalization;
using UserRequestsKafkaGenerator.Services;

namespace UserRequestsKafkaGenerator.Services;

public sealed class CommandLoop
{
    private readonly WorkScheduler _scheduler;

    public CommandLoop(WorkScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        PrintHelp();

        while (!ct.IsCancellationRequested)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLowerInvariant();

            try
            {
                switch (cmd)
                {
                    case "help":
                        PrintHelp();
                        break;

                    case "add":
                        if (parts.Length < 4)
                        {
                            Console.WriteLine("Usage: add <userId> <endpoint> <rpm>");
                            break;
                        }
                        var item = _scheduler.Add(
                            int.Parse(parts[1]),
                            parts[2],
                            int.Parse(parts[3])
                        );
                        Console.WriteLine($"Added: {item}");
                        break;

                    case "list":
                        foreach (var it in _scheduler.List())
                            Console.WriteLine(it);
                        break;

                    case "update":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Usage: update <id> [rpm=<int>] [endpoint=<str>]");
                            break;
                        }
                        if (!Guid.TryParse(parts[1], out var id))
                        {
                            Console.WriteLine("Invalid GUID.");
                            break;
                        }
                        int? rpm = null;
                        string? endpoint = null;
                        foreach (var token in parts.Skip(2))
                        {
                            if (token.StartsWith("rpm=")) rpm = int.Parse(token[4..]);
                            if (token.StartsWith("endpoint=")) endpoint = token[9..];
                        }
                        Console.WriteLine(_scheduler.Update(id, rpm, endpoint)
                            ? "Updated."
                            : "Not found.");
                        break;

                    case "remove":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Usage: remove <id>");
                            break;
                        }
                        if (Guid.TryParse(parts[1], out var rid) && _scheduler.Remove(rid))
                            Console.WriteLine("Removed.");
                        else
                            Console.WriteLine("Not found.");
                        break;

                    case "quit":
                    case "exit":
                        return;

                    default:
                        Console.WriteLine("Unknown command. Type 'help'.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            await Task.Yield();
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            Commands:
            add <userId> <endpoint> <rpm>
            list
            update <id> [rpm=<int>] [endpoint=<str>]
            remove <id>
            help
            quit
        """);
    }
}
