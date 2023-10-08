using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine;

namespace UltraFunGuns
{
    public class ExplosiveVoxel : IVoxelState
    {

        [Configgy.Configgable("UltraFunGuns/Voxel/TNT")]
        private static float activateForce = 3f;

        [Configgy.Configgable("UltraFunGuns/Voxel/TNT")]
        private static float explosionSizeMultiplier = 1f;

        private Rigidbody rb;
        private Voxel voxel;
        private Explodable explodable;
        private bool primed;

        public ExplosiveVoxel() 
        { }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {

        }

        public object GetStateData()
        {
            return null;
        }

        public void OnVoxel(Voxel voxel)
        {
            this.voxel = voxel;
        }

        public void OnVoxelBreak(Voxel voxel)
        {

        }

        public void OnVoxelDestroyed(Voxel voxel)
        {

        }

        public void OnVoxelInteract(Voxel voxel)
        {
            if (primed)
                return;

            Prime(4f);

            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle;
            Vector3 velocity = new Vector3(randomDirection.x, 4f, randomDirection.y).normalized * activateForce;
            rb.velocity += velocity;
        }

        


        private void Prime(float time)
        {
            primed = true;
            //Clear world position.
            VoxelWorld.SetVoxel(voxel.GetLocation(), null);
            rb = voxel.gameObject.AddComponent<Rigidbody>();
            voxel.StartCoroutine(Timer(()=>explodable.Explode(null), time));
        }


        private IEnumerator Timer(Action onTimerFinish, float timer)
        {
            while(timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            onTimerFinish?.Invoke();
        }

        public void OnVoxelPlaced(Voxel voxel)
        {
            explodable = voxel.gameObject.AddComponent<Explodable>();

            explodable.OnExplode += (e) =>
            {
                //if explosion isnt null, it means weve been exploded by another explosion. So we should skip the majority of the fuse and add velocity
                if(e != null)
                {
                    if (e.harmless)
                        return;

                    Prime(0.1f);
                    Vector3 towardsExplodable = (voxel.transform.position - e.transform.position).normalized * 25f;
                    rb.velocity += towardsExplodable;
                    return;
                }

                //Actual explosion
                GameObject ukExplosion = GameObject.Instantiate(Prefabs.UK_Explosion.Asset, voxel.transform.position, Quaternion.identity);
                Explosion exp = ukExplosion.GetComponentInChildren<Explosion>();
                if(exp != null)
                    exp.maxSize *= (VoxelWorld.WorldScale / 2f) * explosionSizeMultiplier;
                GameObject.Destroy(voxel.gameObject);
            };
        }

        public void OnVoxelUpdate(Voxel voxel)
        {

        }

        public void PrintState(Voxel voxel)
        {

        }

        public void SetStateData(object stateData)
        {

        }
    }
}
