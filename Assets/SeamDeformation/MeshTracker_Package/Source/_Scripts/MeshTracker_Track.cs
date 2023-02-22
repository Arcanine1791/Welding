using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MeshTracker
{
    // MeshTracker - Track source.
    // The script allows you to create custom tracks on objects with MeshTracker_Object behaviour.
    // Requires at least one object with MeshTracker_Object component in the scene.
    // Author: Matej Vanco, originally written in 2018, updated in 2022.
    [AddComponentMenu("Matej Vanco/Mesh Tracker/Mesh Tracker Track")]
    public class MeshTracker_Track : MonoBehaviour
    {
        #region Variables

        public bool mmt_TargetedToCPUBasedType = false;

        //Raycast init
        private RaycastHit mmt_Hit;
        private Ray mmt_Ray;
        public Vector3 mmt_HitLocationHistory;

        //Track layer init
        [System.Serializable]
        public class Mt_TrackLayer
        {
            public string mmt_TrackName = "";
            public Color mmt_TrackColor = Color.white;

            //Basic Track Settings
            public float mmt_TrackSize = 150;
            public Texture mmt_TrackGraphic;
            public Material mmt_TrackBrush;
            public bool mmt_CopyObjectRotation = false;
            public bool mmt_UseSmartLayerRotation = false;

            public enum Mmt_GetRotationType { X, Y, Z};
            public Mmt_GetRotationType mmt_GetRotationType = Mmt_GetRotationType.Y;

            public Mmt_Direction mmt_UpwardDirection = Mmt_Direction.Forward;

            public bool mmt_InverseSmartLayerRotation = false;
            public bool mmt_FixObjectScaleWithTrackSize = false;
            public bool mmt_GetScaleFromRoot = false;
            public float mmt_InternalScaleMultiplier = 1;

            [System.Serializable]
            public class Mt_TrackEffects
            {
                public float mmt_MotionSpeed = 2.0f;
                public bool mmt_LinearMotion = false;
                public float mmt_MotionDrag = 0.5f;

                public bool mmt_ChangeTrackSize = false;
                public float mmt_StartTrackSize = 100.0f;
                public float mmt_EndTrackSize = 0.0f;

                public bool mmt_ChangeBrushOpacity = false;
                [Range(0.0f,1.0f)] public float mmt_StartBrushOpacity = 1.0f;
                [Range(0.0f, 1.0f)] public float mmt_EndBrushOpacity = 0.0f;

                public Vector3 mmt_Direction = new Vector3(1, 0, 0);
                public Vector3 mmt_DirectionDouble = new Vector3(-1, 0, 0);
                public bool mmt_SmartTrackRotation = true;
                public bool mmt_LocalSpace = true;

                public bool mmt_useDoubleCast = false;
                public bool mmt_useTrackReferences = true;
                public Texture mmt_TrackGraphic;
                public Material mmt_TrackBrush;

                public bool mmt_adjustLifetimebyObjectSpeed = false;
                public float mmt_EffectLifetime = 5.0f;

                protected Mt_TrackEffects(Mt_TrackEffects sender)
                {
                    mmt_MotionSpeed = sender.mmt_MotionSpeed;
                    mmt_LinearMotion = sender.mmt_LinearMotion;
                    mmt_MotionDrag = sender.mmt_MotionDrag;

                    mmt_ChangeTrackSize = sender.mmt_ChangeTrackSize;
                    mmt_StartTrackSize = sender.mmt_StartTrackSize;
                    mmt_EndTrackSize = sender.mmt_EndTrackSize;

                    mmt_ChangeBrushOpacity = sender.mmt_ChangeBrushOpacity;
                    mmt_StartBrushOpacity = sender.mmt_StartBrushOpacity;
                    mmt_EndBrushOpacity = sender.mmt_EndBrushOpacity;

                    mmt_Direction = sender.mmt_Direction;
                    mmt_SmartTrackRotation = sender.mmt_SmartTrackRotation;
                    mmt_LocalSpace = sender.mmt_LocalSpace;

                    mmt_useDoubleCast = sender.mmt_useDoubleCast;
                    mmt_DirectionDouble = sender.mmt_DirectionDouble;
                    mmt_useTrackReferences = sender.mmt_useTrackReferences;
                    mmt_TrackGraphic = sender.mmt_TrackGraphic;
                    mmt_TrackBrush = sender.mmt_TrackBrush;

                    mmt_adjustLifetimebyObjectSpeed = sender.mmt_adjustLifetimebyObjectSpeed;
                    mmt_EffectLifetime = sender.mmt_EffectLifetime;
                }
                public Mt_TrackEffects Clone()
                {
                    return new Mt_TrackEffects(this);
                }
            }


            public bool mmt_enabledTrackEffects = false;
            public Mt_TrackEffects mmt_trackEffects;

            //Additional Controls
            public bool mmt_MobilePlatform = false;
            public bool mmt_RayOriginIsCursor = false;
            public Camera mmt_CameraTarget;
            public bool mmt_InputEvent = false;
            public KeyCode mmt_InputKey = KeyCode.Mouse0;

            //Direction
            public bool mmt_RayDirectionWorldSpace = true;
            public enum Mmt_Direction { Down, Up, Right, Left, Forward, Backward };
            public Mmt_Direction mmt_RayDirection;
            public float mmt_RayDistance = 10;
            public Vector3 mmt_RayOriginOffset = new Vector3(0, 0, 0);

            //Conditions
            public LayerMask mmt_AllowedLayers = -1;

            public bool mmt_AllObjectsAllowed = true;
            public string mmt_AllowedWithTag;

            public bool mmt_UseSpeedLimits = true;
            public float mmt_SpeedLimitMin = 1.0f;
            public float mmt_SpeedLimitMax = 100.0f;

            //Events
            public bool mmt_EnableEvents = false;

            public bool mmt_Event_UpdateEventEveryFrame = false;
            public UnityEvent mmt_Event_SurfaceDetected;
            public UnityEvent mmt_Event_SurfaceExit;
            public bool Event_Enter;

            //Debug
            public bool mmt_Debug_ShowDebugGraphic = false;
        }

        public List<Mt_TrackLayer> TrackLayer = new List<Mt_TrackLayer>();

        //Update init
        public bool mmt_AlwaysCheckSurface = true;
        public bool mmt_UseInterval = true;
        public float mmt_Interval = 2;

        //Track 'effect' buffer init
        public struct STrackBuffer
        {
            public bool processing;

            public int trackReference;

            public float trackSize;
            public float brushOpacity;
            public Texture trackGraphic;
            public Material trackBrush;

            public Vector3 trackPos;
            public Vector3 trackDirection;
            public float trackDrag;

            public float trackLifetime;
            public float trackTimer;
        }
        public bool mmt_useEffects = false;
        public enum EffectQuality { Low, Medium, High, Extreme, Custom };
        public EffectQuality mmt_effectQuality = EffectQuality.High;
        public int mmt_MaxTrackBuffers { get; protected set; }
        public int mmt_CustomTrackBuffers = 5;
        private STrackBuffer[] mmt_TrackBuffer;

        //Additional internal
        private float mmt_timer;

        public float mmt_speed;
        public Vector3 mmt_previousPos;

        private bool mmt_effectProcessorStarted = false;

        #endregion

        private void Awake()
        {
            //Creating a quality buffer for the effects (if possible)
            if (!mmt_useEffects) return;

            switch (mmt_effectQuality)
            {
                case EffectQuality.Low:
                    mmt_MaxTrackBuffers = 30;
                    break;
                case EffectQuality.Medium:
                    mmt_MaxTrackBuffers = 120;
                    break;
                case EffectQuality.High:
                    mmt_MaxTrackBuffers = 350;
                    break;
                case EffectQuality.Extreme:
                    mmt_MaxTrackBuffers = 500;
                    break;
                case EffectQuality.Custom:
                    mmt_MaxTrackBuffers = Mathf.Clamp(mmt_CustomTrackBuffers, 1, 1000);
                    break;
            }
            mmt_TrackBuffer = new STrackBuffer[mmt_MaxTrackBuffers];
        }

        private void Start()
        {
            mmt_previousPos = transform.position;
        }

        private void OnEnable()
        {
            if (!mmt_useEffects) return;
            if (mmt_effectProcessorStarted) return;
            StartCoroutine(Fin_effectProcessor());
            mmt_effectProcessorStarted = true;
        }

        private void OnDisable()
        {
            if (!mmt_useEffects) return;
            StopAllCoroutines();
            mmt_effectProcessorStarted = false;
        }

        private void Update()
        {
            //----Calculate Speed
            float calSpeed = (mmt_previousPos - transform.position).magnitude / Time.deltaTime;
            mmt_speed = Mathf.Lerp(mmt_speed, calSpeed, Time.deltaTime * 24.0f);
            mmt_speed = Mathf.Round(mmt_speed * 100f) / 100f;

            if (mmt_AlwaysCheckSurface) 
                F_DetectSurface();
            else if (mmt_UseInterval)
            {
                mmt_timer += Time.deltaTime;
                if (mmt_timer > mmt_Interval)
                {
                    F_DetectSurface();
                    mmt_timer = 0;
                }
            }
            mmt_previousPos = Vector3.Lerp(mmt_previousPos, transform.position, 0.5f);
        }

        private Mesh m;
        private Material mat;
        private void OnDrawGizmosSelected()
        {
            if (TrackLayer.Count == 0)
                return;

            //Visualize track direction and track graphic (if possible)
            Gizmos.color = Color.cyan;

            foreach (Mt_TrackLayer tlayer in TrackLayer)
            {
                if (!tlayer.mmt_Debug_ShowDebugGraphic)
                    continue;
                if (tlayer.mmt_RayOriginIsCursor)
                    continue;

                Vector3 Dir = GetDirection(tlayer.mmt_RayDirection, tlayer.mmt_RayDirectionWorldSpace);
                Gizmos.DrawLine(transform.position + tlayer.mmt_RayOriginOffset, transform.position + tlayer.mmt_RayOriginOffset + Dir * tlayer.mmt_RayDistance);

                if (tlayer.mmt_TrackGraphic == null)
                    continue;

                if(m == null)
                {
                    GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    m = newPlane.GetComponent<MeshFilter>().sharedMesh;
                    DestroyImmediate(newPlane);
                }
                if(mat == null)
                    mat = new Material(Shader.Find("Transparent/Diffuse"));
                else
                    mat.mainTexture = tlayer.mmt_TrackGraphic;

                mat.SetPass(1);
                float scale = tlayer.mmt_TrackSize;
                if (tlayer.mmt_InternalScaleMultiplier == 0)
                    tlayer.mmt_InternalScaleMultiplier = 1;
                if (tlayer.mmt_FixObjectScaleWithTrackSize)
                {
                    if (tlayer.mmt_GetScaleFromRoot)
                        scale = transform.root.localScale.x * tlayer.mmt_InternalScaleMultiplier;
                    else
                        scale = transform.localScale.x * tlayer.mmt_InternalScaleMultiplier;
                }
                Quaternion rot = transform.rotation;
                if (!tlayer.mmt_CopyObjectRotation && !tlayer.mmt_UseSmartLayerRotation)
                    rot = Quaternion.identity;
                Graphics.DrawMesh(m, Matrix4x4.TRS(transform.position + tlayer.mmt_RayOriginOffset + Dir * tlayer.mmt_RayDistance, rot, (Vector3.one * scale) / 950), mat,0);
            }
        }

        /// <summary>
        /// Process raycast and detect surface - if the surface is detected, track will be proceeded
        /// </summary>
        public void F_DetectSurface()
        {
            if (TrackLayer.Count == 0)
                return;

            //---Looping through all track layers
            for(int iii = 0; iii < TrackLayer.Count; iii++)
            {
                Mt_TrackLayer trackLayer = TrackLayer[iii];

                if (trackLayer.mmt_InternalScaleMultiplier == 0)
                    trackLayer.mmt_InternalScaleMultiplier = 1;

                //---Creating raycast origin
                if (!trackLayer.mmt_RayOriginIsCursor)
                {
                    mmt_Ray = new Ray(transform.position
                        + (transform.forward * trackLayer.mmt_RayOriginOffset.z)
                        + (transform.right * trackLayer.mmt_RayOriginOffset.x)
                        + (transform.up * trackLayer.mmt_RayOriginOffset.y),
                        GetDirection(trackLayer.mmt_RayDirection, trackLayer.mmt_RayDirectionWorldSpace));
                }
                else
                {
                    if (trackLayer.mmt_CameraTarget == null)
                    {
                        trackLayer.mmt_CameraTarget = Camera.main;
                        if (!trackLayer.mmt_CameraTarget)
                        {
                            Debug.LogError("Camera Target is missing in MeshTracker_Track! Layer Name: " + trackLayer.mmt_TrackName);
                            this.enabled = false;
                            return;
                        }
                    }
                    if (trackLayer.mmt_MobilePlatform)
                    {
                        if (Input.touchCount > 0)
                        {
                            for (int i = 0; i < Input.touchCount; i++) 
                                mmt_Ray = trackLayer.mmt_CameraTarget.ScreenPointToRay(Input.GetTouch(i).position);
                        }
                        else continue;
                    }
                    else
                    {
                        mmt_Ray = trackLayer.mmt_CameraTarget.ScreenPointToRay(Input.mousePosition);
                        if (trackLayer.mmt_InputEvent && !Input.GetKey(trackLayer.mmt_InputKey))
                            continue;
                    }
                }

                mmt_Hit = new RaycastHit();
                bool RaycastResult = Physics.Raycast(mmt_Ray, out mmt_Hit, trackLayer.mmt_RayDistance, trackLayer.mmt_AllowedLayers);

                Paintable p=GameObject.FindGameObjectWithTag("Edge").GetComponent<Paintable>();

                if (RaycastResult)
                {
                    PaintManager.instance.paint(p, this.transform.position, Player.instance.radius, Player.instance.hardness, Player.instance.strength, Player.instance.paintColor);
                    //---Checking the conditions first
                    if (!mmt_Hit.collider && !mmt_Hit.collider.GetComponent<MeshCollider>())
                    {
                        Fin_SendExitEventIfPossible(trackLayer);
                        continue;
                    }
                    if (!trackLayer.mmt_AllObjectsAllowed && mmt_Hit.collider.tag != trackLayer.mmt_AllowedWithTag)
                    {
                        Fin_SendExitEventIfPossible(trackLayer);
                        continue;
                    }
                    if (!trackLayer.mmt_RayOriginIsCursor && trackLayer.mmt_UseSpeedLimits)
                    {
                        if (!(mmt_speed > trackLayer.mmt_SpeedLimitMin && mmt_speed < trackLayer.mmt_SpeedLimitMax))
                        {
                            Fin_SendExitEventIfPossible(trackLayer);
                            continue;
                        }
                    }

                    //---Creating raycast hit storage
                    mmt_HitLocationHistory = mmt_Hit.point;

                    Texture sendTrack = trackLayer.mmt_TrackGraphic;
                    if (sendTrack == null && !mmt_TargetedToCPUBasedType)
                    {
                        Debug.LogError("Track Graphic is missing in Track Layer array element! Track Name: "+trackLayer.mmt_TrackName);
                        continue;
                    }

                    //---Setting the correct track size
                    float RotTrack = 0;
                    if (trackLayer.mmt_FixObjectScaleWithTrackSize)
                    {
                        if (trackLayer.mmt_GetScaleFromRoot)
                            trackLayer.mmt_TrackSize = transform.root.localScale.x * trackLayer.mmt_InternalScaleMultiplier;
                        else
                            trackLayer.mmt_TrackSize = transform.localScale.x * trackLayer.mmt_InternalScaleMultiplier;
                    }

                    //---Setting the track rotation
                    if (!trackLayer.mmt_UseSmartLayerRotation && trackLayer.mmt_CopyObjectRotation)
                    {
                        switch(trackLayer.mmt_GetRotationType)
                        {
                            case Mt_TrackLayer.Mmt_GetRotationType.X:
                                RotTrack = transform.rotation.eulerAngles.x;
                                break;
                            case Mt_TrackLayer.Mmt_GetRotationType.Y:
                                RotTrack = transform.rotation.eulerAngles.y;
                                break;
                            case Mt_TrackLayer.Mmt_GetRotationType.Z:
                                RotTrack = transform.rotation.eulerAngles.z;
                                break;
                        }
                    }
                    else if (trackLayer.mmt_UseSmartLayerRotation)
                    {
                        if ((mmt_previousPos - transform.position) != Vector3.zero)
                        {
                            Vector3 tdir = GetDirection(trackLayer.mmt_RayDirection, trackLayer.mmt_RayDirectionWorldSpace);
                            float angle = GetSignedAngle(GetDirection(trackLayer.mmt_UpwardDirection, trackLayer.mmt_RayDirectionWorldSpace), mmt_previousPos - transform.position, -tdir);
                            RotTrack = angle + (trackLayer.mmt_InverseSmartLayerRotation ? 180 : 0);
                        }
                    }

                    if (mmt_useEffects && trackLayer.mmt_enabledTrackEffects)
                    { 
                        F_QueueForEffect(trackLayer, iii); 
                        if(trackLayer.mmt_trackEffects.mmt_useDoubleCast)
                            F_QueueForEffect(trackLayer, iii, true);
                    }

                    //---Sending the track attributes to target hit
                    MeshTracker_Object hitTarget = mmt_Hit.collider.transform.GetComponent<MeshTracker_Object>();
                    if (hitTarget)
                    {
                        if (hitTarget.mmt_UseGPUbasedType)
                            hitTarget.FGPUbased_CreateTrack(mmt_Hit.textureCoord, trackLayer.mmt_TrackSize, sendTrack, trackLayer.mmt_TrackBrush, RotTrack);
                        else
                            hitTarget.FCPUbased_CreateTrack(mmt_Hit.point, trackLayer.mmt_TrackSize, hitTarget.mmt_trackParamsCPUbased.mmt_Direction);
                    }
                    else if (mmt_Hit.collider.transform.parent && mmt_Hit.collider.transform.parent.GetComponent<MeshTracker_Object>())
                    {
                        hitTarget = mmt_Hit.collider.transform.parent.GetComponent<MeshTracker_Object>();
                        if (hitTarget.mmt_UseGPUbasedType)
                            hitTarget.FGPUbased_CreateTrack(mmt_Hit.textureCoord, trackLayer.mmt_TrackSize, sendTrack, trackLayer.mmt_TrackBrush, RotTrack);
                        else
                            hitTarget.FCPUbased_CreateTrack(mmt_Hit.point, trackLayer.mmt_TrackSize, hitTarget.mmt_trackParamsCPUbased.mmt_Direction);
                    }
                    else if (mmt_Hit.collider.transform.root.GetComponent<MeshTracker_Object>())
                    {
                        hitTarget = mmt_Hit.collider.transform.root.GetComponent<MeshTracker_Object>();
                        if (hitTarget.mmt_UseGPUbasedType)
                            hitTarget.FGPUbased_CreateTrack(mmt_Hit.textureCoord, trackLayer.mmt_TrackSize, sendTrack, trackLayer.mmt_TrackBrush, RotTrack);
                        else
                            hitTarget.FCPUbased_CreateTrack(mmt_Hit.point, trackLayer.mmt_TrackSize, hitTarget.mmt_trackParamsCPUbased.mmt_Direction);
                    }

                    //---Processing events
                    if (trackLayer.mmt_EnableEvents)
                    {
                        if (!trackLayer.mmt_Event_UpdateEventEveryFrame)
                        {
                            if (!trackLayer.Event_Enter)
                            {
                                if (trackLayer.mmt_Event_SurfaceDetected != null)
                                    trackLayer.mmt_Event_SurfaceDetected.Invoke();

                                trackLayer.Event_Enter = true;
                            }
                        }
                        else if (trackLayer.mmt_Event_SurfaceDetected != null)
                            trackLayer.mmt_Event_SurfaceDetected.Invoke();
                    }
                }
                else
                {
                    Fin_SendExitEventIfPossible(trackLayer);
                    continue;
                }
            }
        }

        /// <summary>
        /// Process exit event if possible
        /// </summary>
        /// <param name="tlayer">sender</param>
        private void Fin_SendExitEventIfPossible(Mt_TrackLayer tlayer)
        {
            if (!tlayer.mmt_EnableEvents)
                return;

            if (tlayer.Event_Enter)
            {
                if (tlayer.mmt_Event_SurfaceExit != null)
                    tlayer.mmt_Event_SurfaceExit.Invoke();
                tlayer.Event_Enter = false;
            }
        }

        #region Track Effects Processor

        public void F_QueueForEffect(Mt_TrackLayer layer, int index, bool doubler = false)
        {
            if (mmt_TrackBuffer.Length == 0) return;

            for (int i = 0; i < mmt_TrackBuffer.Length; i++)
            {
                STrackBuffer b = mmt_TrackBuffer[i];

                if (b.processing) 
                    continue;

                b.trackPos = mmt_HitLocationHistory + Vector3.up * 0.1f;
                b.processing = true;
                b.trackTimer = 0;
                b.trackLifetime = layer.mmt_trackEffects.mmt_adjustLifetimebyObjectSpeed ? Mathf.Clamp(layer.mmt_trackEffects.mmt_EffectLifetime / mmt_speed, 0, 10) : Mathf.Clamp(layer.mmt_trackEffects.mmt_EffectLifetime, 0.01f, 10);
                b.trackDrag = layer.mmt_trackEffects.mmt_LinearMotion ? 0 : layer.mmt_trackEffects.mmt_MotionDrag == 0 ? 1 : layer.mmt_trackEffects.mmt_MotionDrag;
                b.trackDirection = layer.mmt_trackEffects.mmt_SmartTrackRotation
                    ? Vector3.Cross(Quaternion.Euler(!doubler ? layer.mmt_trackEffects.mmt_Direction : layer.mmt_trackEffects.mmt_DirectionDouble) * GetMoveDirection(), layer.mmt_RayDirectionWorldSpace ? Vector3.up : transform.up)
                    : layer.mmt_trackEffects.mmt_LocalSpace ? Quaternion.Euler(!doubler ? layer.mmt_trackEffects.mmt_Direction : layer.mmt_trackEffects.mmt_DirectionDouble) * transform.forward : 
                    !doubler ? layer.mmt_trackEffects.mmt_Direction : layer.mmt_trackEffects.mmt_DirectionDouble;
                b.trackSize = layer.mmt_TrackSize;
                b.brushOpacity = layer.mmt_trackEffects.mmt_ChangeBrushOpacity ? layer.mmt_trackEffects.mmt_StartBrushOpacity : 0;
                if(!layer.mmt_trackEffects.mmt_useTrackReferences)
                {
                    b.trackBrush = layer.mmt_trackEffects.mmt_TrackBrush;
                    b.trackGraphic = layer.mmt_trackEffects.mmt_TrackGraphic;
                }
                else
                {
                    b.trackBrush = layer.mmt_TrackBrush;
                    b.trackGraphic = layer.mmt_TrackGraphic;
                }
                b.trackReference = index;

                mmt_TrackBuffer[i] = b;
                break;
            }
        }

        private IEnumerator Fin_effectProcessor()
        {
            while (true)
            {
                for (int i = 0; i < mmt_TrackBuffer.Length; i++)
                {
                    STrackBuffer t = mmt_TrackBuffer[i];
                    if (!t.processing) 
                        continue;

                    t.trackPos += (t.trackDrag != 0 ? t.trackDrag : 1.0f) * Time.deltaTime * TrackLayer[t.trackReference].mmt_trackEffects.mmt_MotionSpeed * t.trackDirection;

                    if (TrackLayer[t.trackReference].mmt_trackEffects.mmt_ChangeTrackSize)
                        t.trackSize = Mathf.Lerp(TrackLayer[t.trackReference].mmt_trackEffects.mmt_StartTrackSize, TrackLayer[t.trackReference].mmt_trackEffects.mmt_EndTrackSize, t.trackTimer / t.trackLifetime);
                    
                    t.trackDrag = Mathf.Clamp01(t.trackDrag - (t.trackDrag * Time.deltaTime));

                    if (TrackLayer[t.trackReference].mmt_trackEffects.mmt_ChangeBrushOpacity)
                    {
                        t.brushOpacity = Mathf.Lerp(TrackLayer[t.trackReference].mmt_trackEffects.mmt_StartBrushOpacity, TrackLayer[t.trackReference].mmt_trackEffects.mmt_EndBrushOpacity, t.trackTimer / t.trackLifetime);
                        t.trackBrush.SetFloat("_Opacity", t.brushOpacity);
                    }

                    Fin_TryCasts(t);

                    t.trackTimer += Time.deltaTime;
                    if (t.trackTimer >= t.trackLifetime)
                        t.processing = false;
                    mmt_TrackBuffer[i] = t;
                }
                yield return null;
            }
        }

        private readonly RaycastHit[] hitNoAllocs = new RaycastHit[1];
        private RaycastHit hit;
        private void Fin_TryCasts(STrackBuffer sender)
        {
            Ray r = new Ray(sender.trackPos, GetDirection(TrackLayer[sender.trackReference].mmt_RayDirection, TrackLayer[sender.trackReference].mmt_RayDirectionWorldSpace));
            if (Physics.RaycastNonAlloc(r, hitNoAllocs, TrackLayer[sender.trackReference].mmt_RayDistance, TrackLayer[sender.trackReference].mmt_AllowedLayers) == 0) return;

            hit = hitNoAllocs[0];
            MeshTracker_Object obj = hit.collider.gameObject.GetComponent<MeshTracker_Object>();

            if (!obj) return;
            if (!TrackLayer[sender.trackReference].mmt_AllObjectsAllowed && !obj.CompareTag(TrackLayer[sender.trackReference].mmt_AllowedWithTag)) return;

            if(obj.mmt_UseGPUbasedType)
                obj.FGPUbased_CreateTrack(hit.textureCoord, sender.trackSize, sender.trackGraphic, sender.trackBrush);
            else
                obj.FCPUbased_CreateTrack(hit.point, sender.trackSize, obj.mmt_trackParamsCPUbased.mmt_Direction);
        }

        #endregion

        #region Additional methods

        private Vector3 GetDirection(Mt_TrackLayer.Mmt_Direction dir, bool WorldSpace)
        {
            Vector3 Dir = Vector3.zero;
            switch (dir)
            {
                case Mt_TrackLayer.Mmt_Direction.Up:
                    Dir = WorldSpace ? Vector3.up : transform.up;
                    break;
                case Mt_TrackLayer.Mmt_Direction.Down:
                    Dir = WorldSpace ? -Vector3.up : -transform.up;
                    break;

                case Mt_TrackLayer.Mmt_Direction.Right:
                    Dir = WorldSpace ? Vector3.right : transform.right;
                    break;
                case Mt_TrackLayer.Mmt_Direction.Left:
                    Dir = WorldSpace ? -Vector3.right : -transform.right;
                    break;

                case Mt_TrackLayer.Mmt_Direction.Forward:
                    Dir = WorldSpace ? Vector3.forward : transform.forward;
                    break;
                case Mt_TrackLayer.Mmt_Direction.Backward:
                    Dir = WorldSpace ? -Vector3.forward : -transform.forward;
                    break;
            }
            return Dir;
        }

        private Vector3 GetMoveDirection()
        {
            return (mmt_previousPos - transform.position).normalized;
        }

        private float GetSignedAngle(Vector3 From, Vector3 A, Vector3 Upwards)
        {
            return Mathf.Atan2(Vector3.Dot(Upwards.normalized, Vector3.Cross(From, A)), Vector3.Dot(From, A)) * Mathf.Rad2Deg;
        }

        #endregion
    }
}