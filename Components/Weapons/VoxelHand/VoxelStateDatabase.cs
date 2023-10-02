using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    public static class VoxelStateDatabase
    {

        private static Dictionary<string, Type> stateRegistry;

        public static Type GetStateType(string id)
        {
            if (stateRegistry == null)
                InitializeStates();

            if(!stateRegistry.ContainsKey(id))
                return null;

            return stateRegistry[id];
        }

        private static void InitializeStates()
        {
            stateRegistry = new Dictionary<string, Type>();
            stateRegistry.Add("glass", typeof(VoxelGlass));
            stateRegistry.Add("tnt", typeof(ExplosiveVoxel));
        }

    }
}
