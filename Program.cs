using Pastel;
using System.Collections.Immutable;

Console.WriteLine("Starting RGB Snake...".Pastel(ConsoleColor.White));
Console.WriteLine();

try
{
    ImmutableArray<IFrontend> frontends =
        [
            new CorsairRAMiCUEFrontend(),
            new DebugFrontend(4, 10),
            new DebugFrontend(8, 20)
        ];

    Console.WriteLine("Select frontend:");
    for (int i = 0; i < frontends.Length; i++)
    {
        Console.WriteLine($" {i + 1}. {frontends[i].Name}");
    }
    Console.WriteLine(" 0. Exit");
    Console.WriteLine();

    while (true)
    {
        Console.Write("Select option: ");
        var input = Console.ReadLine()?.Trim();
        if (input is null || input == "0")
        {
            break;
        }
        else if (int.TryParse(input, out var option) && option >= 1 && option <= frontends.Length)
        {
            var snake = new Snake(frontends[option - 1].GetScreen());
            Console.TreatControlCAsInput = true;

            Console.WriteLine("Started!");
            Console.WriteLine();
            Console.WriteLine("Press the Escape (Esc) key to exit");
            Console.WriteLine();

            snake.Start();
            break;
        }
    }

}
catch (Exception ex)
{
    Console.WriteLine("Game crashed :(".Pastel(ConsoleColor.Red));
    Console.WriteLine(ex);
}
finally
{
    Console.WriteLine();
    Console.WriteLine("Exiting...");
}
