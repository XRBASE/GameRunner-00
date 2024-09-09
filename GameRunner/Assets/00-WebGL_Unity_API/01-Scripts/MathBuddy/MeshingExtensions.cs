using MathBuddy.Vectors;
using UnityEngine;
using UnityEngine.Rendering;

namespace MathBuddy.Meshing
{
    /// <summary>
    /// Extension class for mesh functions.
    /// </summary>
    public static class MeshingExtensions
    {
        /// <summary>
        /// Get triangle mesh (y- flat side, z- broad side), with rounded underside
        /// </summary>
        /// <param name="tris">amount of triangles used for broad side (roundness of broad side).</param>
        public static Mesh GetRoundedTri(float length, float angle, int tris)
        {
            //only work with positive numbers above one
            if (tris < 1) tris = 1;
            angle = Mathf.Max(1f, Mathf.Abs(angle));
            Vector3[] vertices = new Vector3[3 + tris - 1];

            //minus one ensures the whole angle is met
            float aStep = angle / tris;
            vertices[0] = Vector3.zero;
            Vector2 fwd = Vector2.up * length;
            float a = -(angle / 2f);

            //leftmost vertex
            vertices[1] = VectorExtensions.Rotate(fwd, a);
            vertices[1].z = vertices[1].y;
            vertices[1].y = 0f;
            for (int i = 1; i < (tris + 1); i++) {
                //plus one accounts for the tip
                vertices[i + 1] = VectorExtensions.Rotate(fwd, a + (aStep * i));
                vertices[i + 1].z = vertices[i + 1].y;
                vertices[i + 1].y = 0f;
            }

            ushort[] indices = new ushort[tris * 3];
            for (ushort i = 0; i < tris; i++) {
                indices[i * 3] = (ushort) (i + 2);
                indices[i * 3 + 1] = (ushort) (i + 1);
                indices[i * 3 + 2] = 0;
            }

            Mesh m = new Mesh();
            m.vertices = vertices;
            m.indexFormat = IndexFormat.UInt16;
            m.SetIndices(indices, MeshTopology.Triangles, 0);

            m.RecalculateNormals();
            return m;
        }

        /// <summary>
        /// Get mesh in rectangular shape (2D with flat side in y direction). GetRekt son!
        /// </summary>
        /// <param name="size">size of the rectangle (y will be mapped to z axis).</param>
        /// <param name="pivotCentered">should the pivot be in the center of at the minimal corner of the rect.</param>
        public static Mesh GetRect(Vector2 size, bool pivotCentered = false)
        {
            Vector3[] vertices;
            if (pivotCentered) {
                Vector2 hSize = size / 2f;
                vertices = new[] {
                    new Vector3(-hSize.x, 0f, -hSize.y), new Vector3(-hSize.x, 0f, hSize.y),
                    new Vector3(hSize.x, 0f, -hSize.y), new Vector3(hSize.x, 0f, hSize.y)
                };
            }
            else {
                vertices = new[] {
                    new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, size.y),
                    new Vector3(size.x, 0f, 0f), new Vector3(size.x, 0f, size.y)
                };
            }

            ushort[] indices = new ushort[] {
                0, 1, 2, 2, 1, 3
            };


            Mesh m = new Mesh();
            m.vertices = vertices;
            m.indexFormat = IndexFormat.UInt16;
            m.SetIndices(indices, MeshTopology.Triangles, 0);

            m.RecalculateNormals();
            return m;
        }
    }
}