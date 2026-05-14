using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Roguelike
{
    public class RoomBuilder : MonoBehaviour
    {
        [Header("Level Settings (Tiles)")]
        public int mapWidth = 60;
        public int mapHeight = 45;
        public float cellSize = 1.0f;
        public int roomCount = 8;
        public Vector2Int minRoomSize = new Vector2Int(8, 6);
        public Vector2Int maxRoomSize = new Vector2Int(12, 10);
        public int corridorWidth = 2;
        [Range(0, 1)] public float grassDensity = 0.1f;

        [Header("Tile Assignments")]
        public TileBase floorTile;
        public TileBase wallTile;
        public TileBase wallTileH;
        public TileBase wallTileV;
        public TileBase wallTileTL;
        public TileBase wallTileTR;
        public TileBase wallTileBL;
        public TileBase wallTileBR;
        public TileBase grassTile;
        public TileBase groundTile;

        [Header("References")]
        public Tilemap floorTilemap;
        public Tilemap wallTilemap;
        public Tilemap grassTilemap;
        public Tilemap groundTilemap;
        public PlayerSpawnResolver spawnResolver;
        public EnemySpawner enemySpawner;
        public Transform enemyContainer;

        private LevelGridData gridData;
        private List<RectInt> rooms = new List<RectInt>();

        private void Start()
        {
            BuildLevel();
        }

        [ContextMenu("Build Level")]
        public void BuildLevel()
        {
            if (floorTilemap == null || wallTilemap == null) return;

            gridData = new LevelGridData(mapWidth, mapHeight);
            rooms.Clear();

            Grid gridComponent = floorTilemap.layoutGrid;
            if (gridComponent != null) gridComponent.cellSize = new Vector3(cellSize, cellSize, 1);

            int attempts = 0;
            while (rooms.Count < roomCount && attempts < 1000)
            {
                attempts++;
                
                int w = Random.Range(minRoomSize.x, maxRoomSize.x);
                int h = Random.Range(minRoomSize.y, maxRoomSize.y);
                int x = Random.Range(5, mapWidth - w - 5);
                int y = Random.Range(5, mapHeight - h - 5);

                RectInt newRoom = new RectInt(x, y, w, h);

                int padding = 2;
                RectInt checkRect = new RectInt(x - padding, y - padding, w + padding * 2, h + padding * 2);
                
                bool overlaps = false;
                foreach (var r in rooms)
                {
                    if (checkRect.Overlaps(r))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    gridData.AddRoom(newRoom);
                    
                    if (rooms.Count > 0)
                    {
                        Vector2Int start = rooms[rooms.Count - 1].center();
                        Vector2Int end = newRoom.center();
                        int corrRadius = Mathf.Max(1, corridorWidth / 2);
                        gridData.AddCorridor(start, end, corrRadius);
                    }

                    rooms.Add(newRoom);
                }
            }

            gridData.GenerateWalls();

            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();
            if (grassTilemap != null) grassTilemap.ClearAllTiles();
            if (groundTilemap != null) groundTilemap.ClearAllTiles();

            if (enemyContainer != null)
            {
                for (int i = enemyContainer.childCount - 1; i >= 0; i--)
                {
                    if (Application.isPlaying)
                        Destroy(enemyContainer.GetChild(i).gameObject);
                    else
                        DestroyImmediate(enemyContainer.GetChild(i).gameObject);
                }
            }

            var floorPosArray = new List<Vector3Int>();
            var floorTileArray = new List<TileBase>();
            var wallPosArray = new List<Vector3Int>();
            var wallTileArray = new List<TileBase>();
            var grassPosArray = new List<Vector3Int>();
            var grassTileArray = new List<TileBase>();
            var groundPosArray = new List<Vector3Int>();
            var groundTileArray = new List<TileBase>();

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    CellType type = gridData.Cells[x, y];
                    Vector3Int pos = new Vector3Int(x, y, 0);

                    if (groundTile != null)
                    {
                        groundPosArray.Add(pos);
                        groundTileArray.Add(groundTile);
                    }

                    if (type == CellType.Empty)
                    {
                        if (grassTile != null && Random.value < grassDensity)
                        {
                            grassPosArray.Add(pos);
                            grassTileArray.Add(grassTile);
                        }
                    }
                    else if (type == CellType.Floor)
                    {
                        floorPosArray.Add(pos);
                        floorTileArray.Add(floorTile);
                    }
                    else if (type == CellType.Wall)
                    {
                        wallPosArray.Add(pos);
                        
                        TileBase selectedWall = wallTile; // Default to Top Slab
                        
                        if (wallTileV != null)
                        {
                            // 2.5D Perspective Logic:
                            // If there is floor BELOW this wall cell, it's the FRONT FACE of a northern wall.
                            bool floorBelow = y > 0 && gridData.Cells[x, y - 1] == CellType.Floor;
                            
                            if (floorBelow) 
                            {
                                selectedWall = wallTileV; // Assign Face sprite
                            }
                        }
                        
                        wallTileArray.Add(selectedWall);
                    }
}
            }

            if (grassTilemap != null) grassTilemap.SetTiles(grassPosArray.ToArray(), grassTileArray.ToArray());
            if (groundTilemap != null) groundTilemap.SetTiles(groundPosArray.ToArray(), groundTileArray.ToArray());
            floorTilemap.SetTiles(floorPosArray.ToArray(), floorTileArray.ToArray());
            wallTilemap.SetTiles(wallPosArray.ToArray(), wallTileArray.ToArray());

            if (spawnResolver != null)
            {
                if (rooms.Count > 0)
                {
                    Vector2Int center = rooms[0].center();
                    spawnResolver.ResolveSpawnAt(new Vector3Int(center.x, center.y, 0));
                }
                else
                {
                    spawnResolver.ResolveSpawn(gridData);
                }
            }

            if (enemySpawner != null && gridComponent != null)
            {
                for (int i = 1; i < rooms.Count; i++)
                {
                    int count = Random.Range(1, 3);
                    for (int j = 0; j < count; j++)
                    {
                        Vector2Int spawnCell = new Vector2Int(
                            Random.Range(rooms[i].xMin + 1, rooms[i].xMax - 1),
                            Random.Range(rooms[i].yMin + 1, rooms[i].yMax - 1)
                        );
                        Vector3 worldPos = gridComponent.GetCellCenterWorld((Vector3Int)spawnCell);
                        enemySpawner.SpawnAt(worldPos, enemyContainer);
                    }
                }
            }
        }
    }

    public static class RectIntExtensions
    {
        public static Vector2Int center(this RectInt rect)
        {
            return new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);
        }
    }
}
