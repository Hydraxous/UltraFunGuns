namespace UltraFunGuns
{

    [System.Serializable]
    public class SaveInfo : Validatable
    {
        public override bool AllowExternalRead => true;

        public bool firstTimeUsingInventory;
        public bool firstTimeModLoaded;
        public string modVersion;
        public int basketballHighScore;
        public bool usedVoxelhandBefore;

        public SaveInfo()
        {
            this.modVersion = ConstInfo.RELEASE_VERSION;
            this.firstTimeModLoaded = true;
            this.firstTimeUsingInventory = true;
            this.basketballHighScore = 0;
            this.usedVoxelhandBefore = false;
        }

        public override bool Validate()
        {
            if (modVersion == null)
            {
                return false;
            }

            if (modVersion == "")
            {
                return false;
            }

            //If the version is mismatched with the save files, regenerate all files.
            if (modVersion != ConstInfo.RELEASE_VERSION)
            {
                //DataManager.Config.New();
                //DataManager.Loadout.New();
                //DataManager.Keybinds.New();
                return false;
            }

            return true;
        }
    }
}
