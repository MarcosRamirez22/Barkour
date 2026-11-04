
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkDecorationSpawner : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap ground;
    [SerializeField] private Tilemap obstacles;
    [SerializeField] private Tilemap decorationsBack;
    [SerializeField] private Tilemap decorationsFront;

    [Header("Decoration Tiles")]
    [SerializeField] private TileBase[] backTiles;
    [SerializeField] private TileBase[] frontTiles;

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.20f;

    [Range(0f, 1f)]
    [SerializeField] private float frontChance = 0.20f;

    [Min(1)]
    [SerializeField] private int minimumSpacing = 2;

    public void Generate(int runSeed, int chunkIndex)
    {
        if (ground == null || decorationsBack == null ||
            decorationsFront == null)
        {
            Debug.LogWarning("Decoration Tilemaps are missing!");
            return;
        }

        // Separate deterministic randomness from level generation.
        int decorationSeed = unchecked(runSeed * 31 + chunkIndex * 7919);
        System.Random random = new System.Random(decorationSeed);

        decorationsBack.ClearAllTiles();
        decorationsFront.ClearAllTiles();

        BoundsInt bounds = ground.cellBounds;
        int lastPlacedX = int.MinValue / 2;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            if (x - lastPlacedX < minimumSpacing)
                continue;

            // Find the highest exposed ground tile in this column.
            for (int y = bounds.yMax - 1; y >= bounds.yMin; y--)
            {
                Vector3Int groundCell = new Vector3Int(x, y, 0);
                Vector3Int decorationCell = new Vector3Int(x, y + 1, 0);

                if (!ground.HasTile(groundCell))
                    continue;

                if (ground.HasTile(decorationCell))
                    continue;

                // Avoid decorations near obstacles.
                bool blocked = false;

                if (obstacles != null)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (obstacles.HasTile(
                                new Vector3Int(x + dx, y + 1, 0)) ||
                            obstacles.HasTile(
                                new Vector3Int(x + dx, y + 2, 0)))
                        {
                            blocked = true;
                            break;
                        }
                    }
                }

                if (blocked)
                    continue;

                if (random.NextDouble() >= spawnChance)
                    break;

                bool useFront = frontTiles != null &&
                                frontTiles.Length > 0 &&
                                ((backTiles == null || backTiles.Length == 0) ||
                                 random.NextDouble() < frontChance);

                TileBase[] choices = useFront ? frontTiles : backTiles;

                if (choices == null || choices.Length == 0)
                    break;

                TileBase selected = choices[random.Next(choices.Length)];

                if (selected != null)
                {
                    Tilemap target = useFront
                        ? decorationsFront
                        : decorationsBack;

                    target.SetTile(decorationCell, selected);
                    lastPlacedX = x;
                }

                break;
            }
        }
    }
}
