using HydraDynamics.DataPersistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [System.Serializable]
    public class Config : Validatable
    {
        public override bool AllowExternalRead => false;

        //Generic
        public bool DebugMode;
        public bool DisableVersionMessages;
        public bool EnableVisualizer;
        public bool EnableAutosave;
        public bool EnableBasketballHoop;
        public bool EnableWeaponsInAllScenes;

        //Weapon values
        public bool BasketBallMode;
        public bool TricksniperReactionsEnabled;

        //UI
        public float MouseOverNodeTime;
        public float InventoryInfoCardScale;

        public Dictionary<string, object> Configgables;

        public Config()
        {
            this.DisableVersionMessages = false;
            this.BasketBallMode = false;
            this.MouseOverNodeTime = 0.8f;
            this.DebugMode = false;
            this.EnableVisualizer = false;
            this.InventoryInfoCardScale = 1.0f;
            this.EnableAutosave = true;
            this.EnableBasketballHoop = true;
            this.TricksniperReactionsEnabled = true;
            Configgables = new Dictionary<string, object>();
        }

        public override bool Validate()
        {
            if (Configgables == null)
                return false;

            return true;
        }
    }
}
