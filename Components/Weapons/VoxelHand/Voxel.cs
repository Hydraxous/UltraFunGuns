using System;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns.Components;
using UnityEngine;

namespace UltraFunGuns
{
    public class Voxel : MonoBehaviour
    {
        public VoxelData VoxelData { get; private set; }
        public IVoxelState VoxelState { get; private set; }

        private void Start()
        {
            VoxelState?.OnVoxelPlaced(this);
        }

        public void SetVoxelData(VoxelData voxelData)
        {
            this.VoxelData = voxelData;
            GetComponent<Renderer>().sharedMaterial = voxelData.Material;
        }

        public void SetVoxelState(IVoxelState voxelState)
        {
            VoxelState = voxelState;
            VoxelState?.OnVoxel(this);
        }

        public void Break()
        {
            //Spawn break particle 
            BreakVFX();
            VoxelState?.OnVoxelBreak(this);
            VoxelWorld.DeleteVoxel(GetLocation());
        }

        public void Interact()
        {
            VoxelState?.OnVoxelInteract(this);
        }

        public void VoxelUpdate()
        {
            VoxelState?.OnVoxelUpdate(this);
        }

        public void PrintState()
        {
            VoxelState?.PrintState(this);
        }

        private void BreakVFX()
        {
            if (VoxelData.Sound != null)
                AudioSource.PlayClipAtPoint(VoxelData.Sound, transform.position);

            if (VoxelData.Material == null)
                return;

            GameObject newParticle = GameObject.Instantiate(VoxelDatabase.VoxelParticle, transform.position, Quaternion.identity);
            newParticle.transform.localScale = Vector3.one * VoxelWorld.WorldScale;
            newParticle.layer = 24;
            newParticle.AddComponent<DeleteAfterTime>().TimeLeft = 3f;
            ParticleSystemRenderer psr = newParticle.GetComponent<ParticleSystemRenderer>();
            psr.material = VoxelData.Material;
        }

        public static Voxel Create(VoxelLocation position, VoxelData data, IVoxelState state = null)
        {
            GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newBlock.name = $"VH_{position.Coordinate}";
            newBlock.tag = "Floor";
            newBlock.layer = 24;
            newBlock.transform.position = position.Position;
            newBlock.transform.localScale = Vector3.one * VoxelWorld.WorldScale;

            Voxel newVoxel = newBlock.AddComponent<Voxel>();
            VoxelWorld.ReplaceVoxel(position, newVoxel);



            if(state == null)
            {
                Type stateType = VoxelStateDatabase.GetStateType(data.ID);
                if (stateType != null)
                    if (typeof(IVoxelState).IsAssignableFrom(stateType))
                        state = (IVoxelState)Activator.CreateInstance(stateType);
            }

            newVoxel.SetVoxelData(data);
            newVoxel.SetVoxelState(state);
            return newVoxel;
        }

        public VoxelLocation GetLocation()
        {
            return new VoxelLocation(transform.position);
        }

        private void OnDestroy()
        {
            VoxelState?.OnVoxelDestroyed(this);
        }
    }
}
