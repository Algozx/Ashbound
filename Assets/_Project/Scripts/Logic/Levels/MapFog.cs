using System;

namespace Ashbound.Logic.Levels
{
    /// <summary>Remembers which cells of a map the player has seen.</summary>
    public sealed class MapFog
    {
        private readonly bool[] revealed;

        public MapFog(int width, int height)
        {
            Width = width;
            Height = height;
            revealed = new bool[width * height];
        }

        public int Width { get; }

        public int Height { get; }

        public bool IsRevealed(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height && revealed[y * Width + x];
        }

        /// <summary>Reveals a circle and reports whether anything new was seen.</summary>
        public bool Reveal(int centerX, int centerY, int radius)
        {
            bool changed = false;
            int minY = Math.Max(0, centerY - radius);
            int maxY = Math.Min(Height - 1, centerY + radius);
            int minX = Math.Max(0, centerX - radius);
            int maxX = Math.Min(Width - 1, centerX + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radius * radius && !revealed[y * Width + x])
                    {
                        revealed[y * Width + x] = true;
                        changed = true;
                    }
                }
            }

            return changed;
        }
    }
}
