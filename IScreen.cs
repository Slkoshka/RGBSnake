using Point = (int X, int Y);
using Color = (float R, float G, float B);

interface IScreen
{
    int Width { get; }
    int Height { get; }
    void SetColor(Point position, Color color);
    void Flush();

    public void Fill(Color color)
    {
        for (int y = 0; y < Height; y++)
        {
            for(int x = 0; x < Width; x++)
            {
                SetColor((x, y), color);
            }
        }
    }
}
