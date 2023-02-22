using UnityEngine;

namespace MeshTracker
{
    // MeshTracker - Procedural Plane generator.
    // The script allows you to generate procedural plane with customizable size & resolution. Mostly usable for Mobile platform [IOS & ANDROID].
    // Author: Matej Vanco, originally written in 2018, updated in 2022.
    [ExecuteInEditMode]    
    [AddComponentMenu("Matej Vanco/Mesh Tracker/Procedural Plane")]
    public class MeshTracker_ProceduralPlane : MonoBehaviour
    {
        //---Mesh Plane Variables
        public float Plane_length = 1f;
        public float Plane_width = 1f;
        [Range(2,200)]
        public int Plane_resX = 2;

        public bool UpdateMeshEveryFrame = false;
        public bool GenerateMeshColliderAfterStart = true;

        private MeshFilter meshFilter;

        public void Awake()
        {
            if (!meshFilter && GetComponent<MeshFilter>())
                meshFilter = GetComponent<MeshFilter>();
            if (!Application.isPlaying)
                return;
            if (GenerateMeshColliderAfterStart && !gameObject.GetComponent<MeshCollider>())
                gameObject.AddComponent<MeshCollider>();
        }

        private void Update()
        {
            if (!UpdateMeshEveryFrame) return;
            GenerateMesh();
        }

        private static void GenerateMaterial(GameObject target)
        {
            Shader shad = Shader.Find("Standard");
            Material mat = new Material(shad);
            target.GetComponent<Renderer>().material = mat;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/3D Object/Mesh Tracker/Procedural Plane")]
        public static GameObject GenerateGlobal()
        {
            GameObject gm = new GameObject("MeshTracker_Plane");
            UnityEditor.Selection.activeGameObject = gm;
            gm.AddComponent<MeshTracker_ProceduralPlane>().GenerateMesh();
            return gm;
        }
#endif
        /// <summary>
        /// Generate mesh method
        /// </summary>
        public void GenerateMesh()
        {
            if (meshFilter == null)
            {
                if (!GetComponent<MeshFilter>())
                    gameObject.AddComponent<MeshFilter>();
                if (!GetComponent<MeshRenderer>())
                    gameObject.AddComponent<MeshRenderer>();
                meshFilter = GetComponent<MeshFilter>();
                GenerateMaterial(gameObject);
                return;
            }

            float length = Plane_length;
            float width = Plane_width;
            int res = Plane_resX;

            Mesh mesh = new Mesh();
            int resZ = res;

            length = Mathf.Clamp(length, 1, length);
            width = Mathf.Clamp(width, 1, width);
            res = Mathf.Clamp(res, 1, res);

            Vector3[] vertices = new Vector3[res * resZ];
            for (int z = 0; z < resZ; z++)
            {
                float zPos = ((float)z / (resZ - 1) - .5f) * length;
                for (int x = 0; x < res; x++)
                {
                    float xPos = ((float)x / (res - 1) - .5f) * width;
                    vertices[x + z * res] = new Vector3(xPos, 0f, zPos);
                }
            }

            Vector3[] normals = new Vector3[vertices.Length];
            for (int n = 0; n < normals.Length; n++)
                normals[n] = Vector3.up;
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int v = 0; v < resZ; v++)
            {
                for (int u = 0; u < res; u++)
                {
                    uvs[u + v * res] = new Vector2((float)u / (res - 1), (float)v / (resZ - 1));
                }
            }
            int nbFaces = (res - 1) * (resZ - 1);
            int[] triangles = new int[nbFaces * 6];
            int t = 0;
            for (int face = 0; face < nbFaces; face++)
            {
                int i = face % (res - 1) + (face / (resZ - 1) * res);

                triangles[t++] = i + res;
                triangles[t++] = i + 1;
                triangles[t++] = i;

                triangles[t++] = i + res;
                triangles[t++] = i + res + 1;
                triangles[t++] = i + 1;
            }
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }
    }
}