using System.Collections.Generic;
using System.Diagnostics;
using ScriptableObjects;
using Unity.Mathematics;
using UnityEngine;
using WorldGeneration;
using Debug = UnityEngine.Debug;

namespace Chunks
{
    public class ChunkManager : MonoBehaviour
    {
        [SerializeField] GenerationSettings generationSettings;
        [SerializeField] Material material;

        private readonly HashSet<Chunk> _chunks = new ();
        private MeshGenerator _meshGenerator;
        // Rolled once per session so tweaking other settings doesn't reroll the world
        private int _randomSeed;
        private bool _settingsChanged;

        void Awake()
        {
            this._randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            this._meshGenerator = this.CreateMeshGenerator();
        }

        void OnEnable()
        {
            this.generationSettings.Changed += this.OnSettingsChanged;
        }

        void OnDisable()
        {
            this.generationSettings.Changed -= this.OnSettingsChanged;
        }

        void Start()
        {
            var stopwatch = Stopwatch.StartNew();

            this._chunks.Add(this.CreateChunk(new int3(0, 0, 0)));
            this._chunks.Add(this.CreateChunk(new int3(0, 1, 0)));

            Debug.Log($"Generated {this._chunks.Count} chunks " +
                      $"in {stopwatch.Elapsed.TotalMilliseconds:F1} ms, seed {this._meshGenerator.Seed}");
        }

        void Update()
        {
            // Rebuilt here instead of in the event handler: Unity doesn't allow heavy work inside OnValidate,
            // and dragging a slider fires several changes per frame that this collapses into one rebuild
            if (!this._settingsChanged) return;
            this._settingsChanged = false;

            this.RegenerateChunks();
        }

        private void OnSettingsChanged() => this._settingsChanged = true;

        private MeshGenerator CreateMeshGenerator() =>
            new(this.generationSettings, this.generationSettings.Seed ?? this._randomSeed);

        private void RegenerateChunks()
        {
            var stopwatch = Stopwatch.StartNew();

            this._meshGenerator = this.CreateMeshGenerator();
            foreach (var chunk in this._chunks)
                chunk.Regenerate(this._meshGenerator);

            Debug.Log($"Regenerated {this._chunks.Count} chunks " +
                      $"in {stopwatch.Elapsed.TotalMilliseconds:F1} ms, seed {this._meshGenerator.Seed}");
        }

        private Chunk CreateChunk(int3 chunkCoord)
        {
            var go = new GameObject($"Chunk {chunkCoord}");
            go.transform.SetParent(this.transform, false);

            var chunk = go.AddComponent<Chunk>();
            chunk.Initialize(chunkCoord, this._meshGenerator, this.material);

            return chunk;
        }
    }
}
