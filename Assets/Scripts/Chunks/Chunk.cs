using Unity.Mathematics;
using UnityEngine;
using WorldGeneration;

namespace Chunks
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        private MeshGenerator _meshGenerator;
        private int3 _chunkCoord;
        private Mesh _mesh;

        public void Initialize(int3 chunkCoord, MeshGenerator generator, Material material)
        {
            this._chunkCoord = chunkCoord;

            this._mesh = new Mesh { name = $"Chunk {chunkCoord}" };
            this.GetComponent<MeshFilter>().sharedMesh = this._mesh;

            this.GetComponent<MeshRenderer>().material = material;

            this.Regenerate(generator);
        }

        // Reuses the existing Mesh so live setting changes don't allocate a new one each time
        public void Regenerate(MeshGenerator generator)
        {
            this._meshGenerator = generator;
            this.transform.localPosition = generator.Grid.ChunkOrigin(this._chunkCoord);

            ChunkMeshData meshData = generator.GenerateMeshData(this._chunkCoord);

            this._mesh.Clear();
            this._mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            this._mesh.SetVertices(meshData.Vertices);
            this._mesh.SetTriangles(meshData.Triangles, 0);
            this._mesh.SetNormals(meshData.Normals);
        }

        private void OnDestroy()
        {
            if (this._mesh != null) Destroy(this._mesh);
        }

        private void OnDrawGizmos()
        {
            ChunkGrid grid = this._meshGenerator.Grid;
            float3 origin = grid.ChunkOrigin(this._chunkCoord);
            Gizmos.matrix = this.transform.localToWorldMatrix;

            foreach (var pointCoord in grid.Points())
            {
                float3 localPos = grid.PointToLocal(pointCoord);
                float density = this._meshGenerator.DensityAt(origin + localPos);

                Gizmos.color = new Color(density, density, density, 1.0f);
                Gizmos.DrawSphere(localPos, 0.1f);
            }
        }
    }
}
