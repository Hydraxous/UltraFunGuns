namespace UltraFunGuns
{
    public abstract class DiceGunSpell
    {
        protected int spellPower;
        
        public void ExecuteSpell(DiceGun diceGun)
        {
            ExecuteSpellCore(diceGun);
        }

        protected abstract void ExecuteSpellCore(DiceGun diceGun);
        
        public void OnSpellRolled(int spellPower)
        {
            OnSpellRolledCore(spellPower);
        }

        protected virtual void OnSpellRolledCore(int spellPower)
        {
            this.spellPower = spellPower;
        }

        public void OnSpellAddedToPool(DiceGun diceGun)
        {
            OnSpellRolledCore(spellPower);
        }

        protected virtual void OnSpellAddedToPoolCore(DiceGun diceGun) {}

        public void OnSpellDiscarded()
        {
            OnSpellDiscardedCore();
        }

        protected virtual void OnSpellDiscardedCore() { }
    }
}
