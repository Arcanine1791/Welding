using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace MeshTracker
{
    // MeshTracker - Object source.
    // The script allows you to modify an object which represents a 'base' of interactive surface and the source for MeshTracker_Track components.
    // Author: Matej Vanco, originally written in 2018, updated in 2022.
    [AddComponentMenu("Matej Vanco/Mesh Tracker/Mesh Tracker Object")]
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshTracker_Object : MonoBehaviour
    {
        public bool mmt_UseGPUbasedType = true;

        //----Type = GPU Based
        #region Type_GPU_Based_(Shader)

        [System.Serializable]
        public class TrackParams_GPUbased
        {
            public bool mmt_GenerateCustomCanvas = false;
            [Range(0.0f, 1.0f)] public float mmt_CustomCanvasTone = 0.5f;
            public Texture mmt_Canvas;

            public bool mmt_SurfaceRepairShader = false;
            public float mmt_SurfaceRepairShaderSpeed = 0.001f;
            public float mmt_SurfaceRepairShaderInterval = 0.1f;
            public Texture2D mmt_CanvasRepairCopy;
            public Material mmt_RepairBrushType;

            public enum Mt_CanvasQuality { x128, x256, x512, x1024, x2048, x4096, x8192 };
            public Mt_CanvasQuality mmt_CanvasQuality = Mt_CanvasQuality.x1024;

            public RenderTexture mmt_RenderTex;
            public Material mmt_TrackMaterial;
        }

        public TrackParams_GPUbased mmt_trackParamsGPUbased;

        #endregion

        //----Type = CPU Based
        #region Type_CPU_Based_(Script)

        [System.Serializable]
        public class TrackParams_CPUbased
        {
            public bool mmt_RigidbodiesAllowed = true;

            public bool mmt_MultithreadingSupported = false;
            [Range(1, 30)] public int mmt_ThreadSleep = 10;
            public Thread mmt_Thread;

            public bool mmt_CustomInteractionSpeed = false;
            public bool mmt_ContinuousEffect = false;
            public float mmt_InteractionSpeed = 1.5f;

            public bool mmt_ExponentialDeformation = true;
            public float mmt_InstantRadius = 1.5f;

            public Vector3 mmt_Direction = new Vector3(0, -1, 0);
            public float mmt_Radius = 0.8f;
            public bool mmt_AdjustTrackSizeToInputSize = true;
            public float mmt_MinimumForceDetection = 0;

            public List<Vector3> originalVertices = new List<Vector3>();
            public List<Vector3> storedVertices = new List<Vector3>();
            public List<Vector3> initialVertices = new List<Vector3>();
            public MeshFilter meshF;

            public bool mmt_RepairSurface;
            public float mmt_RepairSpeed = 0.5f;

            public bool mmt_CollideWithSpecificObjects = false;
            public string mmt_CollisionTag = "";

            public bool newRefCreated = false;
        }

        public TrackParams_CPUbased mmt_trackParamsCPUbased;

        #endregion

        //----Collider Settings
        #region Collider Settings

        public bool mmt_Collider_GenerateMeshCollider = true;
        public bool mmt_Collider_Convex = false;
        public MeshColliderCookingOptions mmt_Collider_CookingOptions = MeshColliderCookingOptions.None;
        public Vector3 mmt_Collider_ColliderOffset = new Vector3(0, 0, 0);

        #endregion

        private void Awake()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();

            //---Check basics
            if (!mr)
            {
                Debug.LogError("Mesh Track Object - there is no mesh renderer.");
                this.enabled = false;
                return;
            }
            if (!mr.material)
            {
                Debug.LogError("Mesh Track Object - there is no material.");
                this.enabled = false;
                return;
            }

            if (mmt_UseGPUbasedType)
            {
                //---Create render texture & modify displacement texture
                mmt_trackParamsGPUbased.mmt_TrackMaterial = mr.material;
                int sXY = 1024;
                switch(mmt_trackParamsGPUbased.mmt_CanvasQuality)
                {
                    case TrackParams_GPUbased.Mt_CanvasQuality.x128:
                        sXY = 128;
                        break;
                    case TrackParams_GPUbased.Mt_CanvasQuality.x256:
                        sXY = 256;
                        break;
                    case TrackParams_GPUbased.Mt_CanvasQuality.x512:
                        sXY = 512;
                        break;
                    case TrackParams_GPUbased.Mt_CanvasQuality.x2048:
                        sXY = 2048;
                        break;
                    case TrackParams_GPUbased.Mt_CanvasQuality.x4096:
                        sXY = 4096;
                        break;
                    case TrackParams_GPUbased.Mt_CanvasQuality.x8192:
                        sXY = 8192;
                        break;
                }
                //Generate custom canvas based on selected color tone
                if(mmt_trackParamsGPUbased.mmt_GenerateCustomCanvas)
                {
                    mmt_trackParamsGPUbased.mmt_Canvas = new Texture2D(sXY, sXY, TextureFormat.RGBA32, false);
                    for (int x = 0; x < sXY; x++)
                    {
                        for (int y = 0; y < sXY; y++)
                            ((Texture2D)mmt_trackParamsGPUbased.mmt_Canvas).SetPixel(x, y, 
                                new Color(mmt_trackParamsGPUbased.mmt_CustomCanvasTone,
                                mmt_trackParamsGPUbased.mmt_CustomCanvasTone,
                                mmt_trackParamsGPUbased.mmt_CustomCanvasTone, 1));
                    }
                    ((Texture2D)mmt_trackParamsGPUbased.mmt_Canvas).Apply();
                }
                if (mmt_trackParamsGPUbased.mmt_Canvas == null)
                {
                    Debug.LogError("Mesh Track_Object: starting canvas field is empty.");
                    this.enabled = false;
                    return;
                }
                if (mmt_trackParamsGPUbased.mmt_SurfaceRepairShader) FinGPUbased_CreateRepairTrackCopy((Texture2D)mmt_trackParamsGPUbased.mmt_Canvas);
                mmt_trackParamsGPUbased.mmt_RenderTex = new RenderTexture(sXY, sXY, 0, RenderTextureFormat.ARGBFloat);
                mmt_trackParamsGPUbased.mmt_TrackMaterial.SetTexture("_DispTex", mmt_trackParamsGPUbased.mmt_RenderTex);
                Graphics.Blit(mmt_trackParamsGPUbased.mmt_Canvas, mmt_trackParamsGPUbased.mmt_RenderTex);
            }
            //---Create a new mesh reference if the system is CPU based
            else FinCPUbased_CreateNewMeshReference();

            //---Create a mesh collider if possible
            if (mmt_Collider_GenerateMeshCollider) F_UpdateMeshCollider();
        }

        [ContextMenu("Update Mesh Collider")]
        public void F_UpdateMeshCollider()
        {
            //----Updating MeshCollider----
            MeshCollider col = GetComponent<MeshCollider>();
            if (col) col.convex = mmt_Collider_Convex;
            else
            {
                col = gameObject.AddComponent<MeshCollider>();
                col.convex = mmt_Collider_Convex;
            }

            Mesh Baked_Mesh = new Mesh();

            if (GetComponent<SkinnedMeshRenderer>())
                GetComponent<SkinnedMeshRenderer>().BakeMesh(Baked_Mesh);
            else if (GetComponent<MeshFilter>())
                Baked_Mesh = GetComponent<MeshFilter>().sharedMesh;

            col.sharedMesh = null;
            col.sharedMesh = Baked_Mesh;

            Mesh MeshColliderMesh = new Mesh();
            MeshColliderMesh.vertices = col.sharedMesh.vertices;
            MeshColliderMesh.triangles = col.sharedMesh.triangles;
            MeshColliderMesh.normals = col.sharedMesh.normals;
            MeshColliderMesh.uv = col.sharedMesh.uv;
            MeshColliderMesh.colors = col.sharedMesh.colors;
            Vector3[] verts = MeshColliderMesh.vertices;
            for (int i = 0; i < verts.Length; i++) verts[i] += mmt_Collider_ColliderOffset;
            MeshColliderMesh.vertices = verts;

            col.cookingOptions = mmt_Collider_CookingOptions;

            col.sharedMesh = MeshColliderMesh;
        }

        #region TYPE - - - CPU Based system

        private void Start()
        {
            //Return if GPU based is enabled - GPU based system doesn't need multithreading
            if (mmt_UseGPUbasedType) return;

            if (mmt_trackParamsCPUbased.mmt_MultithreadingSupported && Application.isPlaying)
            {
                Thrd_RealRot = transform.rotation;
                Thrd_RealPos = transform.position;
                Thrd_RealSca = transform.localScale;

                mmt_trackParamsCPUbased.mmt_Thread = new Thread(ThreadWork_ModifyMesh);
                mmt_trackParamsCPUbased.mmt_Thread.Start();
            }
        }

        /// <summary>
        /// Create a new mesh reference (for security reason)
        /// </summary>
        private void FinCPUbased_CreateNewMeshReference()
        {
            if (mmt_trackParamsCPUbased.newRefCreated) return;

            MeshCollider col = GetComponent<MeshCollider>();
            if (!col) col = gameObject.AddComponent<MeshCollider>();

            Mesh NewMesh = new Mesh();
            NewMesh.vertices = col.sharedMesh.vertices;
            NewMesh.triangles = col.sharedMesh.triangles;
            NewMesh.normals = col.sharedMesh.normals;
            NewMesh.uv = col.sharedMesh.uv;
            NewMesh.colors = col.sharedMesh.colors;
            NewMesh.indexFormat = col.sharedMesh.indexFormat;
            col.sharedMesh = NewMesh;

            mmt_trackParamsCPUbased.meshF = GetComponent<MeshFilter>();
            mmt_trackParamsCPUbased.meshF.sharedMesh.MarkDynamic();

            mmt_trackParamsCPUbased.originalVertices.AddRange(mmt_trackParamsCPUbased.meshF.sharedMesh.vertices);
            mmt_trackParamsCPUbased.initialVertices.AddRange(mmt_trackParamsCPUbased.meshF.sharedMesh.vertices);
            mmt_trackParamsCPUbased.storedVertices.AddRange(mmt_trackParamsCPUbased.meshF.sharedMesh.vertices);

            mmt_trackParamsCPUbased.newRefCreated = true;
        }

        private bool checkForUpdate_InterSpeed, checkForUpdate_Repair = false;
        private double timerToRepairSurface;
        private void LateUpdate()
        {
            //Returns if GPU based is enabled
            if (mmt_UseGPUbasedType) return;
            //Returns if multithreading is enabled (it wouldn't make any sense)
            if (mmt_trackParamsCPUbased.mmt_MultithreadingSupported) return;

            //Update 'custom interaction'
            if (mmt_trackParamsCPUbased.mmt_CustomInteractionSpeed)
            {
                if (checkForUpdate_InterSpeed)
                {
                    int doneAll = 0;
                    if (mmt_trackParamsCPUbased.mmt_ContinuousEffect)
                    {
                        for (int i = 0; i < mmt_trackParamsCPUbased.originalVertices.Count; i++)
                        {
                            if (mmt_trackParamsCPUbased.originalVertices[i] == mmt_trackParamsCPUbased.storedVertices[i])
                                doneAll++;
                            mmt_trackParamsCPUbased.originalVertices[i] = Vector3.Lerp(mmt_trackParamsCPUbased.originalVertices[i], mmt_trackParamsCPUbased.storedVertices[i], mmt_trackParamsCPUbased.mmt_InteractionSpeed * Time.deltaTime);
                        }
                        if (doneAll == mmt_trackParamsCPUbased.originalVertices.Count)
                            checkForUpdate_InterSpeed = false;
                        mmt_trackParamsCPUbased.meshF.mesh.SetVertices(mmt_trackParamsCPUbased.originalVertices);
                    }
                    else
                    {
                        List<Vector3> Verts = new List<Vector3>();
                        Verts.AddRange(mmt_trackParamsCPUbased.meshF.mesh.vertices);
                        for (int i = 0; i < Verts.Count; i++)
                        {
                            if (Verts[i] == mmt_trackParamsCPUbased.storedVertices[i])
                                doneAll++;
                            Verts[i] = Vector3.Lerp(Verts[i], mmt_trackParamsCPUbased.storedVertices[i], mmt_trackParamsCPUbased.mmt_InteractionSpeed * Time.deltaTime);
                        }
                        if (doneAll == Verts.Count)
                            checkForUpdate_InterSpeed = false;
                        mmt_trackParamsCPUbased.meshF.mesh.SetVertices(Verts);
                    }

                    mmt_trackParamsCPUbased.meshF.mesh.RecalculateNormals();
                }
            }

            //Update 'repair surface'
            if (mmt_trackParamsCPUbased.mmt_RepairSurface)
            {
                if (checkForUpdate_Repair)
                {
                    int doneAll = 0;
                    for (int i = 0; i < mmt_trackParamsCPUbased.storedVertices.Count; i++)
                    {
                        if (mmt_trackParamsCPUbased.originalVertices[i] == mmt_trackParamsCPUbased.storedVertices[i])
                            doneAll++;
                        mmt_trackParamsCPUbased.storedVertices[i] = Vector3.Lerp(mmt_trackParamsCPUbased.storedVertices[i], mmt_trackParamsCPUbased.initialVertices[i], mmt_trackParamsCPUbased.mmt_RepairSpeed * Time.deltaTime);
                    }
                    if (doneAll == mmt_trackParamsCPUbased.storedVertices.Count)
                        checkForUpdate_Repair = false;
                    if (!mmt_trackParamsCPUbased.mmt_CustomInteractionSpeed)
                    {
                        mmt_trackParamsCPUbased.meshF.mesh.SetVertices(mmt_trackParamsCPUbased.storedVertices);
                        mmt_trackParamsCPUbased.meshF.mesh.RecalculateNormals();
                    }
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!Application.isPlaying)
                return;
            if (mmt_UseGPUbasedType)
                return;
            if (!mmt_trackParamsCPUbased.mmt_RigidbodiesAllowed)
                return;
            if (collision.contacts.Length == 0)
                return;
            if (mmt_trackParamsCPUbased.mmt_MinimumForceDetection != 0 && collision.gameObject.GetComponent<Rigidbody>() &&
                collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude < mmt_trackParamsCPUbased.mmt_MinimumForceDetection)
                return;
            if (mmt_trackParamsCPUbased.mmt_AdjustTrackSizeToInputSize)
                mmt_trackParamsCPUbased.mmt_Radius = collision.transform.localScale.magnitude / 4.0f;
            foreach (ContactPoint cp in collision.contacts)
                FCPUbased_CreateTrack(cp.point, mmt_trackParamsCPUbased.mmt_Radius, mmt_trackParamsCPUbased.mmt_Direction);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!Application.isPlaying)
                return;
            if (mmt_UseGPUbasedType)
                return;
            if (!mmt_trackParamsCPUbased.mmt_RigidbodiesAllowed)
                return;
            if (collision.contacts.Length == 0)
                return;
            if (mmt_trackParamsCPUbased.mmt_MinimumForceDetection != 0 && collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude < mmt_trackParamsCPUbased.mmt_MinimumForceDetection)
                return;

            if (mmt_trackParamsCPUbased.mmt_AdjustTrackSizeToInputSize)
                mmt_trackParamsCPUbased.mmt_Radius = collision.transform.localScale.magnitude / 4;
            foreach (ContactPoint cp in collision.contacts)
                FCPUbased_CreateTrack(cp.point, mmt_trackParamsCPUbased.mmt_Radius, mmt_trackParamsCPUbased.mmt_Direction);
        }

        /// <summary>
        /// Create a track with CPU based system on surface with specific point, size and vertice direction
        /// </summary>
        /// <param name="Point">Enter point of modification</param>
        /// <param name="Radius">Enter interaction radius</param>
        /// <param name="Direction">Enter direction of the vertices</param>
        public void FCPUbased_CreateTrack(Vector3 Point, float Radius, Vector3 Direction)
        {
            if (mmt_trackParamsCPUbased.mmt_AdjustTrackSizeToInputSize == false)
                Radius = mmt_trackParamsCPUbased.mmt_Radius;

            //If multithreading enabled, process multihread
            if (mmt_trackParamsCPUbased.mmt_MultithreadingSupported)
            {
                Thrd_AtPoint = Point;
                Thrd_Radius = Radius;
                Thrd_Dir = Direction;
                Thrd_RealPos = transform.position;
                Thrd_RealRot = transform.rotation;
                Thrd_RealSca = transform.localScale;
            }
            //Otherwise go for the default main thread
            else
            {
                for (int i = 0; i < mmt_trackParamsCPUbased.storedVertices.Count; i++)
                {
                    Vector3 TransformedPoint = transform.TransformPoint(mmt_trackParamsCPUbased.storedVertices[i]);
                    float distance = Vector3.Distance(new Vector3(Point.x,0, Point.z), new Vector3(TransformedPoint.x,0, TransformedPoint.z));
                    if (distance < Radius)
                    {
                        //Modify vertex in specific radius by linear or exponential distance prediction
                        Vector3 modifVertex = mmt_trackParamsCPUbased.originalVertices[i] + (Direction * (mmt_trackParamsCPUbased.mmt_ExponentialDeformation ? (distance > Radius - mmt_trackParamsCPUbased.mmt_InstantRadius ? (Radius - (distance)) : 1) : 1));
                        if (mmt_trackParamsCPUbased.mmt_ExponentialDeformation && ((mmt_trackParamsCPUbased.mmt_Direction.y < 0 ? modifVertex.y > mmt_trackParamsCPUbased.storedVertices[i].y : modifVertex.y < mmt_trackParamsCPUbased.storedVertices[i].y))) continue;
                        mmt_trackParamsCPUbased.storedVertices[i] = modifVertex;
                    }
                }
            }

            //Set vertices & continue
            if (!mmt_trackParamsCPUbased.mmt_CustomInteractionSpeed || mmt_trackParamsCPUbased.mmt_MultithreadingSupported)
                mmt_trackParamsCPUbased.meshF.mesh.SetVertices(mmt_trackParamsCPUbased.storedVertices);
            mmt_trackParamsCPUbased.meshF.mesh.RecalculateNormals();
            checkForUpdate_Repair = true;
            checkForUpdate_InterSpeed = true;
        }

        /// <summary>
        /// Reset current surface (Reset all vertices to the starting position)
        /// </summary>
        public void FCPUbased_ResetSurface()
        {
            for (int i = 0; i < mmt_trackParamsCPUbased.storedVertices.Count; i++)
                mmt_trackParamsCPUbased.storedVertices[i] = mmt_trackParamsCPUbased.initialVertices[i];
            mmt_trackParamsCPUbased.meshF.mesh.SetVertices(mmt_trackParamsCPUbased.storedVertices);
            mmt_trackParamsCPUbased.meshF.mesh.RecalculateNormals();
        }

        //------External thread params----
        private Vector3 Thrd_AtPoint;
        private float Thrd_Radius;
        private Vector3 Thrd_Dir;
        private Vector3 Thrd_RealPos;
        private Vector3 Thrd_RealSca;
        private Quaternion Thrd_RealRot;
        //--------------------------------

        /// <summary>
        /// Main CPU based thread for mesh modification
        /// </summary>
        private void ThreadWork_ModifyMesh()
        {
            while (true)
            {
                for (int i = 0; i < mmt_trackParamsCPUbased.storedVertices.Count; i++)
                {
                    Vector3 TransformedPoint = TransformPoint(Thrd_RealPos, Thrd_RealRot, Thrd_RealSca, mmt_trackParamsCPUbased.storedVertices[i]);
                    float distance = Vector3.Distance(new Vector3(Thrd_AtPoint.x, 0, Thrd_AtPoint.z), new Vector3(TransformedPoint.x, 0, TransformedPoint.z));
                    if (distance < Thrd_Radius)
                    {
                        Vector3 modifVertex = mmt_trackParamsCPUbased.originalVertices[i] + (Thrd_Dir * (mmt_trackParamsCPUbased.mmt_ExponentialDeformation ? (distance > Thrd_Radius - mmt_trackParamsCPUbased.mmt_InstantRadius ? (Thrd_Radius - (distance)) : 1) : 1));
                        if (mmt_trackParamsCPUbased.mmt_ExponentialDeformation && (modifVertex.y > mmt_trackParamsCPUbased.storedVertices[i].y)) continue;
                        mmt_trackParamsCPUbased.storedVertices[i] = modifVertex;
                    }
                }
                Thread.Sleep(mmt_trackParamsCPUbased.mmt_ThreadSleep);
            }
        }

        /// <summary>
        /// Transform point from local to world space utility
        /// </summary>
        private Vector3 TransformPoint(Vector3 WorldPos, Quaternion WorldRot, Vector3 WorldScale, Vector3 Point)
        {
            var localToWorldMatrix = Matrix4x4.TRS(WorldPos, Thrd_RealRot, Thrd_RealSca);
            return localToWorldMatrix.MultiplyPoint3x4(Point);
        }

        private void OnApplicationQuit()
        {
            //Abort thread if possible
            if (mmt_trackParamsCPUbased.mmt_Thread != null && mmt_trackParamsCPUbased.mmt_Thread.IsAlive)
                mmt_trackParamsCPUbased.mmt_Thread.Abort();
        }

        private void OnDestroy()
        {
            //Abort thread if possible
            if (mmt_trackParamsCPUbased.mmt_Thread != null && mmt_trackParamsCPUbased.mmt_Thread.IsAlive)
                mmt_trackParamsCPUbased.mmt_Thread.Abort();
        }

        #endregion

        #region TYPE - - - GPU Based system

        private void Update()
        {
            //Update method is used in case of 'Repair Surface' option only
            if (!mmt_UseGPUbasedType) return;

            if (!mmt_trackParamsGPUbased.mmt_SurfaceRepairShader) return;
            if (mmt_trackParamsGPUbased.mmt_CanvasRepairCopy == null) return;

            //If interval is lower than minimum 'update time' value, create a repair mask every frame without any interval
            if (mmt_trackParamsGPUbased.mmt_SurfaceRepairShaderInterval <= 0.04f)
            {
                FGPUbased_CreateRepairTrack();
                return;
            }
            //Otherwise go for the method in specific interval
            if (timerToRepairSurface > mmt_trackParamsGPUbased.mmt_SurfaceRepairShaderInterval)
            {
                FGPUbased_CreateRepairTrack();
                timerToRepairSurface = 0;
            }
            timerToRepairSurface += Time.deltaTime;
        }

        /// <summary>
        /// Create a repair texture copy (if possible). Should be executed once due to performance
        /// </summary>
        private void FinGPUbased_CreateRepairTrackCopy(Texture2D startingCanvas)
        {
            mmt_trackParamsGPUbased.mmt_CanvasRepairCopy = Instantiate(startingCanvas);
            for (int x = 0; x < startingCanvas.width; x++)
            {
                for (int y = 0; y < startingCanvas.height; y++)
                {
                    Color pix = mmt_trackParamsGPUbased.mmt_CanvasRepairCopy.GetPixel(x, y);
                    pix.a = mmt_trackParamsGPUbased.mmt_SurfaceRepairShaderSpeed;
                    mmt_trackParamsGPUbased.mmt_CanvasRepairCopy.SetPixel(x, y, pix);
                }
            }
            mmt_trackParamsGPUbased.mmt_CanvasRepairCopy.Apply();
        }


        /// <summary>
        /// Create a repair track on the surface (if all required options for 'Track Repair' are assigned)
        /// </summary>
        public void FGPUbased_CreateRepairTrack()
        {
            RenderTexture.active = mmt_trackParamsGPUbased.mmt_RenderTex;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, mmt_trackParamsGPUbased.mmt_RenderTex.width, mmt_trackParamsGPUbased.mmt_RenderTex.height, 0);

            if (mmt_trackParamsGPUbased.mmt_RepairBrushType == null)
                Graphics.DrawTexture(new Rect(0, 0, mmt_trackParamsGPUbased.mmt_RenderTex.width, mmt_trackParamsGPUbased.mmt_RenderTex.height),
                    mmt_trackParamsGPUbased.mmt_CanvasRepairCopy);
            else
                Graphics.DrawTexture(new Rect(0, 0, mmt_trackParamsGPUbased.mmt_RenderTex.width, mmt_trackParamsGPUbased.mmt_RenderTex.height),
                    mmt_trackParamsGPUbased.mmt_CanvasRepairCopy, mmt_trackParamsGPUbased.mmt_RepairBrushType);

            GL.PopMatrix();
            RenderTexture.active = null;
        }

        /// <summary>
        /// Create a track with GPU based system on surface with specific coordinates, size, graphic & additional details
        /// </summary>
        /// <param name="TextureCoords">Enter texture coordinates [example: hit.textureCoord]</param>
        /// <param name="TrackSize">Enter track size</param>
        /// <param name="TrackGraphic">Enter track graphic [ratio must be 1:1]</param>
        /// <param name="TrackBrush">Enter track brush for additional details</param>
        /// <param name="yRotation">Enter additional track rotation in Y axis [if track brush is assigned]</param>
        public void FGPUbased_CreateTrack(Vector2 TextureCoords, float TrackSize, Texture TrackGraphic, Material TrackBrush = null, float yRotation = 0)
        {
            if(!mmt_trackParamsGPUbased.mmt_RenderTex)
            {
                Debug.LogError("Mesh Tracker_Object: there is no render texture.");
                this.enabled = false;
                return;
            }

            RenderTexture.active = mmt_trackParamsGPUbased.mmt_RenderTex;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, mmt_trackParamsGPUbased.mmt_RenderTex.width, mmt_trackParamsGPUbased.mmt_RenderTex.height, 0);

            // Calculating texture-coords for track graphic
            Vector2 coord = new Vector2(TextureCoords.x * mmt_trackParamsGPUbased.mmt_RenderTex.width, 
                mmt_trackParamsGPUbased.mmt_RenderTex.width - TextureCoords.y * mmt_trackParamsGPUbased.mmt_RenderTex.height);

            if (TrackBrush)
            {
                TrackBrush.SetFloat("_Rotation", yRotation);
                Graphics.DrawTexture(new Rect(coord.x - TrackSize / 2, (coord.y - TrackSize / 2), TrackSize, TrackSize), TrackGraphic, TrackBrush);
            }
            else
                Graphics.DrawTexture(new Rect(coord.x - TrackSize / 2, (coord.y - TrackSize / 2), TrackSize, TrackSize), TrackGraphic);

            GL.PopMatrix();
            RenderTexture.active = null;
        }

        /// <summary>
        /// Save current surface canvas (useful in editor) to the specific file path with extension (png only)
        /// </summary>
        /// <param name="FilePath">Example: D://MyCanvas.png or Application.dataPath + "/mycanvas.png"</param>
        public void FGPUbased_SaveCurrentCanvas(string FilePath)
        {
            if (string.IsNullOrEmpty(FilePath)) return;

            Texture mainTexture = mmt_trackParamsGPUbased.mmt_TrackMaterial.GetTexture("_DispTex");
            Texture2D texture2D = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);

            RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
            Graphics.Blit(mainTexture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            //Encode to the higher quality of png format
            byte[] bbb = texture2D.EncodeToPNG();
            try
            {
                System.IO.File.WriteAllBytes(FilePath, bbb);
            }
            catch
            {
                Debug.LogError("MeshTracker_Object: error while writing canvas data");
                return;
            }
        }

        /// <summary>
        /// Reset canvas data & surface heights (removes all colors and depths on the Render Texture)
        /// </summary>
        public void FGPUbased_ClearCanvas()
        {
            if (mmt_trackParamsGPUbased.mmt_RenderTex == null) return;

            RenderTexture.active = mmt_trackParamsGPUbased.mmt_RenderTex;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, mmt_trackParamsGPUbased.mmt_RenderTex.width, mmt_trackParamsGPUbased.mmt_RenderTex.height, 0);

            Graphics.DrawTexture(new Rect(0, 0, mmt_trackParamsGPUbased.mmt_RenderTex.width, mmt_trackParamsGPUbased.mmt_RenderTex.height), mmt_trackParamsGPUbased.mmt_Canvas);

            GL.PopMatrix();
            RenderTexture.active = null;
        }

        #endregion
    }
   
}