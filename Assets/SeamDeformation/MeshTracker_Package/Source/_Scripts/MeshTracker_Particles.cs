using System.Collections.Generic;
using UnityEngine;

namespace MeshTracker
{
    // Additional - MeshTracker - Particles.
    // The script allows you to process mesh tracking on particles collision.
    // Author: Matej Vanco, originally written in 2019, updated in 2022.
    [AddComponentMenu("Matej Vanco/Mesh Tracker/Mesh Tracker Particles")]
    [RequireComponent(typeof(ParticleSystem))]
    public class MeshTracker_Particles : MonoBehaviour
    {
        private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

        public bool CustomTrack = false;

        public MeshTracker_Track TrackPrefab;

        public float TrackSize = 30.0f;
        public Texture TrackGraphic;
        public Material AdditionalBrush;

        public float TrackLifeTime = 1;

        private ParticleSystem part;

        private void Awake()
        {
            if (enabled == false) return;
            collisionEvents = new List<ParticleCollisionEvent>();
            if (!TryGetComponent<ParticleSystem>(out part))
            {
                Debug.LogError("Mesh Tracker Particles - please apply the behaviour to the correct object with particle system");
                this.enabled = false;
                return;
            }
            var c = part.collision;
            c.enabled = true;
            c.sendCollisionMessages = true;
        }

        private RaycastHit rHit;
        private void OnParticleCollision(GameObject other)
        {
            if (enabled == false)
                return;
            int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
            int i = 0;
            while (i < numCollisionEvents)
            {
                if (CustomTrack && TrackPrefab)
                {
                    GameObject newTrack = Instantiate(TrackPrefab.gameObject, collisionEvents[i].intersection + Vector3.up * 0.5f, Quaternion.identity) as GameObject;
                    Destroy(newTrack, TrackLifeTime);
                    newTrack.hideFlags = HideFlags.HideInHierarchy;
                }
                else
                {
                    Vector3 rOrigin = collisionEvents[i].intersection + Vector3.up * 0.5f;
                    Ray r = new Ray(rOrigin, Vector3.down);
                    if (Physics.Raycast(r, out rHit))
                    {
                        if (rHit.collider && rHit.collider.GetComponent<MeshTracker_Object>())
                            rHit.collider.GetComponent<MeshTracker_Object>().FGPUbased_CreateTrack(rHit.textureCoord, TrackSize, TrackGraphic, AdditionalBrush);
                    }
                }
                i++;
            }
        }
    }
}