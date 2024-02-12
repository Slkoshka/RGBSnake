using Point = (int X, int Y);
using Color = (float R, float G, float B);
using System.Collections.Immutable;
using Pastel;

class Snake(IScreen screen)
{
    enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    record Segment(Point Position, Color Color);
    record Food(Point Position, Color Color);
    abstract record GameState()
    {
        public record StartAnimation(int Tick = 0) : GameState;
        public record Gameplay(ImmutableArray<Segment> Segments, Food? Food = null, Direction Direction = Direction.Up, int DelayedSegments = 2) : GameState;
        public record DeathAnimation(Segment Head, ImmutableArray<Segment> Body, int Tick = 0) : GameState;
        public record Exit() : GameState;
    };

    public static readonly TimeSpan TICK_DURATION = TimeSpan.FromSeconds(0.5);
    public static readonly Color COLOR_HEAD = (1.0f, 0.0f, 0.0f);
    public static readonly Color COLOR_BODY = (1.0f, 1.0f, 1.0f);
    public static readonly Color COLOR_FOOD = (0.0f, 1.0f, 0.0f);
    public static readonly Color COLOR_BACKGROUND = (0.0f, 0.0f, 0.0f);

    private static Point SpawnPoint => (0, 0);

    // Shared with the input thread
    private readonly object _lock = new();
    private Direction _newDirection = Direction.Up;
    private GameState _gameState = new GameState.StartAnimation(-1);
    //

    private static Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => throw new Exception(),
        };
    }

    private static Point DirectionToOffset(Direction direction)
    {
        return direction switch
        {
            Direction.Up => (0, 1),
            Direction.Down => (0, -1),
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            _ => throw new Exception(),
        };
    }

    private static Point AddOffset(Point point, Point offset) => (point.X + offset.X, point.Y + offset.Y);

    private bool IsValidHeadPosition(GameState.Gameplay gameplay, Point point)
    {
        return point.X >= 0 && point.Y >= 0 && point.X < screen.Width && point.Y < screen.Height && !gameplay.Segments.Any(segment => segment.Position == point);
    }

    private GameState.Gameplay SpawnFood(GameState.Gameplay gameplay)
    {
        if (gameplay.Food is null)
        {
            var validPoints = Enumerable.Range(0, screen.Width).SelectMany(x => Enumerable.Range(0, screen.Height).Select(y => (X: x, Y: y)).Where(position => !gameplay.Segments.Any(segment => segment.Position == position)));
            return gameplay with { Food = new Food(validPoints.ElementAt(Random.Shared.Next(0, validPoints.Count())), COLOR_FOOD) };
        }
        else
        {
            return gameplay;
        }
    }

    private GameState Update(GameState state, Direction newDirection)
    {
        switch (state)
        {
            case GameState.StartAnimation start:
                return start.Tick > screen.Height / 2 + 3 ? new GameState.Gameplay([new Segment(SpawnPoint, COLOR_HEAD)]) : start with { Tick = start.Tick + 1 };

            case GameState.Gameplay gameplay:
                if (newDirection == GetOppositeDirection(gameplay.Direction))
                {
                    newDirection = gameplay.Direction;
                }

                var newHeadPosition = AddOffset(gameplay.Segments[0].Position, DirectionToOffset(newDirection));
                if (!IsValidHeadPosition(gameplay, newHeadPosition))
                {
                    return new GameState.DeathAnimation(gameplay.Segments[0], [..gameplay.Segments.Skip(1)]);
                }
                var foodEaten = gameplay.Food is not null && gameplay.Segments.Any(segment => segment.Position == gameplay.Food.Position);

                return SpawnFood(new GameState.Gameplay(
                    [
                        gameplay.Segments[0] with { Position = newHeadPosition },
                        ..Enumerable.Range(1, gameplay.Segments.Length - 1).Select(i => gameplay.Segments[i] with { Position = gameplay.Segments[i - 1].Position }),
                        ..(IEnumerable<Segment>)(gameplay.DelayedSegments > 0 || foodEaten ? [new Segment(gameplay.Segments[^1].Position, COLOR_BODY)] : []),
                    ],
                    foodEaten ? null : gameplay.Food,
                    newDirection,
                    foodEaten ? gameplay.DelayedSegments : Math.Max(0, gameplay.DelayedSegments - 1)));

            case GameState.DeathAnimation death:
                return death.Tick > 5 ? new GameState.StartAnimation() : death with { Tick = death.Tick + 1 };

            default:
                return state;
        }
    }

    private void Render(GameState state)
    {
        switch (state)
        {
            case GameState.StartAnimation start:
                screen.Fill(COLOR_BACKGROUND);
                if (start.Tick < screen.Height / 2)
                {
                    for (int x = 0; x < screen.Width; x++)
                    {
                        screen.SetColor((x, start.Tick * 2), COLOR_BODY);
                        if (start.Tick + 1 < screen.Height)
                        {
                            screen.SetColor((x, start.Tick * 2 + 1), COLOR_BODY);
                        }
                    }
                }
                else
                {
                    if ((start.Tick - screen.Height) % 2 == 0)
                    {
                        screen.SetColor(SpawnPoint, COLOR_HEAD);
                    }
                }
                break;

            case GameState.Gameplay gameplay:
                screen.Fill(COLOR_BACKGROUND);
                if (gameplay.Food is not null)
                {
                    screen.SetColor(gameplay.Food.Position, gameplay.Food.Color);
                }
                foreach (var segment in gameplay.Segments)
                {
                    screen.SetColor(segment.Position, segment.Color);
                }
                break;

            case GameState.DeathAnimation death:
                screen.Fill(COLOR_BACKGROUND);
                if (death.Tick % 2 == 0)
                {
                    screen.SetColor(death.Head.Position, death.Head.Color);
                }
                foreach (var segment in death.Body)
                {
                    screen.SetColor(segment.Position, segment.Color);
                }
                break;
        }

        screen.Flush();
    }

    private void InputThreadProc()
    {
        try
        {
            screen.Fill(COLOR_BACKGROUND);

            while (true)
            {
                lock (_lock) if (_gameState is GameState.Exit)
                {
                    break;
                }

                var key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        lock (_lock) _gameState = new GameState.Exit();
                        break;

                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        lock (_lock) _newDirection = Direction.Up;
                        break;

                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        lock (_lock) _newDirection = Direction.Down;
                        break;

                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        lock (_lock) _newDirection = Direction.Left;
                        break;

                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        lock (_lock) _newDirection = Direction.Right;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Input thread crashed :(".Pastel(ConsoleColor.Red));
            Console.WriteLine(ex);
        }
        finally
        {
            lock (_lock) _gameState = new GameState.Exit();
        }
    }

    public void Start()
    {
        var inputThread = new Thread(InputThreadProc);
        inputThread.Start();
        try
        {
            while (true)
            {
                lock (_lock) if (_gameState is GameState.Exit)
                {
                    break;
                }

                try
                {
                    Thread.Sleep(TICK_DURATION);

                    Direction direction;
                    GameState gameState;
                    lock (_lock)
                    {
                        gameState = _gameState;
                        direction = _newDirection;
                    }

                    gameState = Update(gameState, direction);

                    lock (_lock) if (_gameState is not GameState.Exit)
                    {
                        _gameState = gameState;
                        if (_gameState is GameState.Gameplay gameplay)
                        {
                            _newDirection = gameplay.Direction;
                        }
                    }
                    else
                    {
                        break;
                    }

                    Render(gameState);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        finally
        {
            lock (_lock) _gameState = new GameState.Exit();

            if (!inputThread.Join(TimeSpan.FromSeconds(0.5)))
            {
                Console.WriteLine("Press any key to exit");

                if (!inputThread.Join(TimeSpan.FromSeconds(5.0)))
                {
                    Environment.Exit(1);
                }
            }
        }
    }
}
