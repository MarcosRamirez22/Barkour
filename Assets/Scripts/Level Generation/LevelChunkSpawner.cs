
using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelChunkSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameCamera;

    [Header("Chunk Prefabs")]
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private GameObject[] additionalChunkPrefabs;

    [Header("Chunk Settings")]
    [SerializeField] private int startingChunkCount = 5;
    [SerializeField] private int safeStartingChunks = 2;
    [SerializeField] private float chunkWidth = 20f;

    [Header("Generation Settings")]
    [SerializeField] private float spawnAheadDistance = 20f;
    [SerializeField] private float cleanupDistance = 20f;

    [Header("Starting Position")]
    [SerializeField] private float startX = -20f;
    [SerializeField] private float groundY = 0f;

    [Header("Seed Settings")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;

    private System.Random levelRandom;

    private readonly Queue<GameObject> activeChunks =
        new Queue<GameObject>();

    private float nextChunkX;
    private int spawnedChunkCount;

    // Allows other systems to access the current seed.
    public int CurrentSeed => seed;

    private void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;

        if (chunkPrefab == null ||
            gameCamera == null ||
            !gameCamera.orthographic ||
            chunkWidth <= 0f)
        {
            Debug.LogError(
                "LevelChunkSpawner: Invalid configuration!"
            );

            enabled = false;
            return;
        }

        InitializeSeed();

        nextChunkX = startX;
        spawnedChunkCount = 0;

        SpawnStartingChunks();
        GenerateChunksAhead();
    }

    private void LateUpdate()
    {
        GenerateChunksAhead();
        CleanupOldChunks();
    }

    private void InitializeSeed()
    {
        if (useRandomSeed)
        {
            seed = Environment.TickCount;
        }

        levelRandom = new System.Random(seed);

        Debug.Log("Barkour Level Seed: " + seed);
    }

    private void SpawnStartingChunks()
    {
        for (int i = 0; i < startingChunkCount; i++)
        {
            SpawnChunk();
        }
    }


    private void GenerateChunksAhead()
    {
        float cameraRightEdge =
            gameCamera.transform.position.x +
            gameCamera.orthographicSize * gameCamera.aspect;

        float spawnBoundary =
            cameraRightEdge +
            Mathf.Max(0f, spawnAheadDistance);

        while (nextChunkX < spawnBoundary)
        {
            SpawnChunk();
        }
    }

    private GameObject SelectChunkPrefab()
    {
        // Give Kali a safe, flat starting area.
        if (spawnedChunkCount < safeStartingChunks)
        {
            return chunkPrefab;
        }

        // Use the straight chunk if no alternatives exist.
        if (additionalChunkPrefabs == null ||
            additionalChunkPrefabs.Length == 0)
        {
            return chunkPrefab;
        }

        // Deterministic random selection.
        // Index 0 represents the straight chunk.
        int randomIndex = levelRandom.Next(
            additionalChunkPrefabs.Length + 1
        );

        if (randomIndex == 0)
        {
            return chunkPrefab;
        }

        GameObject selectedPrefab =
            additionalChunkPrefabs[randomIndex - 1];

        // Fall back to straight if an array slot is empty.
        return selectedPrefab != null
            ? selectedPrefab
            : chunkPrefab;
    }

    private void SpawnChunk()
    {
        GameObject selectedPrefab = SelectChunkPrefab();

        Vector3 spawnPosition = new Vector3(
            nextChunkX,
            groundY,
            0f
        );

        GameObject newChunk = Instantiate(
            selectedPrefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );

        activeChunks.Enqueue(newChunk);

        spawnedChunkCount++;
        nextChunkX += chunkWidth;
    }

    private void CleanupOldChunks()
    {
        float cameraLeftEdge =
            gameCamera.transform.position.x -
            gameCamera.orthographicSize * gameCamera.aspect;

        float cleanupBoundary =
            cameraLeftEdge -
            Mathf.Max(0f, cleanupDistance);

        while (activeChunks.Count > 0)
        {
            GameObject oldestChunk = activeChunks.Peek();

            float chunkRightEdge =
                oldestChunk.transform.position.x +
                chunkWidth;

            // Keep chunks that are still near the camera.
            if (chunkRightEdge >= cleanupBoundary)
            {
                break;
            }

            activeChunks.Dequeue();
            Destroy(oldestChunk);
        }
    }
}
