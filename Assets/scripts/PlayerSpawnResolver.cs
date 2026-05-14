using UnityEngine;

namespace Roguelike
{
    public class PlayerSpawnResolver : MonoBehaviour
    {
        public GameObject player;

        public void ResolveSpawn(LevelGridData grid)
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
                if (player == null) player = GameObject.Find("player");
            }

            if (player == null) return;

            Vector3Int spawnCell = Vector3Int.zero;
            bool found = false;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (grid.Cells[x, y] == CellType.Floor)
                    {
                        if (x > 0 && x < grid.Width - 1 && y > 0 && y < grid.Height - 1)
                        {
                            if (grid.Cells[x + 1, y] == CellType.Floor && grid.Cells[x, y + 1] == CellType.Floor)
                            {
                                spawnCell = new Vector3Int(x + 1, y + 1, 0);
                                found = true;
                                break;
                            }
                        }
                        
                        if (!found)
                        {
                            spawnCell = new Vector3Int(x, y, 0);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            if (found)
            {
                ResolveSpawnAt(spawnCell);
            }
        }

        public void ResolveSpawnAt(Vector3Int spawnCell)
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
                if (player == null) player = GameObject.Find("player");
            }

            if (player == null) return;

            Grid gridComponent = Object.FindAnyObjectByType<Grid>();
            if (gridComponent != null)
            {
                Vector3 worldPos = gridComponent.GetCellCenterWorld(spawnCell);
                worldPos.z = 0f; // Fix Z position to 0 to avoid being behind walls
                player.transform.position = worldPos;
                
                SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 110;
                }
            }
        }
        }
        }
