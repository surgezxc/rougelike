using UnityEngine;
using System.Collections.Generic;

namespace Roguelike
{
    public enum CellType
    {
        Empty,
        Floor,
        Wall
    }

    public class LevelGridData
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public CellType[,] Cells { get; private set; }

        public LevelGridData(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new CellType[width, height];
        }

        public void AddRoom(RectInt roomRect)
        {
            for (int x = roomRect.xMin; x < roomRect.xMax; x++)
            {
                for (int y = roomRect.yMin; y < roomRect.yMax; y++)
                {
                    if (x >= 0 && x < Width && y >= 0 && y < Height)
                    {
                        Cells[x, y] = CellType.Floor;
                    }
                }
            }
        }

        public void AddCorridor(Vector2Int start, Vector2Int end, int radius)
        {
            int xStart = Mathf.Min(start.x, end.x);
            int xEnd = Mathf.Max(start.x, end.x);
            FillRect(xStart - radius, start.y - radius, xEnd + radius, start.y + radius, CellType.Floor);

            int yStart = Mathf.Min(start.y, end.y);
            int yEnd = Mathf.Max(start.y, end.y);
            FillRect(end.x - radius, yStart - radius, end.x + radius, yEnd + radius, CellType.Floor);
        }

        public void GenerateWalls()
        {
            // Reset everything that isn't floor to Wall first to create a solid mass
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Cells[x, y] == CellType.Empty)
                    {
                        Cells[x, y] = CellType.Wall;
                    }
                }
            }
            
            // Clean up: only keep walls that are near floors to save performance/tiles
            // (Optional, but let's keep it solid for now as requested for 'normal' look)
        }

        private void FillRect(int xMin, int yMin, int xMax, int yMax, CellType type)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if (x >= 0 && x < Width && y >= 0 && y < Height)
                    {
                        Cells[x, y] = type;
                    }
                }
            }
        }
    }
}
