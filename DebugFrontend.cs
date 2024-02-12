using Pastel;
using System.Drawing;

class DebugFrontend(int width, int height) : IFrontend
{
    public string Name => $"Debug mode (output to console, {width}x{height})";

    class Screen(int width, int height) : IScreen
    {
        public int Width => width;
        public int Height => height;
        
        private readonly Color[,] _buffer = new Color[width, height];
        private bool _renderedOnce = false;

        public void SetColor((int X, int Y) position, (float R, float G, float B) color)
        {
            _buffer[position.X, Height - position.Y - 1] = Color.FromArgb((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
        }

        public void Flush()
        {
            if (_renderedOnce)
            {
                Console.SetCursorPosition(0, Console.GetCursorPosition().Top - height);
            }
            else
            {
                _renderedOnce = true;
            }
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Console.Write("#".Pastel(_buffer[x, y]));
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }
    }

    public IScreen GetScreen()
    {
        return new Screen(width, height);
    }

    public void Dispose() { }
}
