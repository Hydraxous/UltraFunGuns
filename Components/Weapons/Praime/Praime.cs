using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{

    //Randomizes your beam when you pick it.
    //Beams have a limited amount of ammo before they get thrown back into the List at a random index.
    //When you run out of ammo you are forced to Reformat
    //Reformatting swaps your beam to a random beam that is not the same or last two beams
    //Beams have different damages, effects, powers, etc
    //OR you can choose the beams and you find them in the levels!!!
    //OR Enemies drop them in levels at a frequency depending on how many beams you have

    [WeaponAbility("Fire","Fire the current configuration.", 0, RichTextColors.red)]
    [WeaponAbility("Reformat","Swith the current configuration.", 1, RichTextColors.lime)]
    [FunGun("ArmCannon", "Praime", 2, true, WeaponIconColor.Red)]
    public class Praime : UltraFunGunBase
    {
        


    }

    public abstract class PraimeBeam
    {
        public abstract bool Initialize();
        public abstract bool CanFire();
        public abstract bool Fire();
        public virtual void OnSwitcOn() { }
        public virtual void OnSwitchOff() { }
    }

    /* Beam ideas
     * 
     * Missile
     * 
     * Plasma Beam
     * Power Beam
     * Ice Beam
     * Wave Beam
     * 
     * Dark Beam
     * Light Beam
     * Annihilator Beam
     * 
     * Phazon Beam
     * Plasa Beam 2.0
     * Nova Beam
     * 
     * Hyper Beam
     * Dread Hyper Beam
     * 
     * 
     * Heavy Beam : Shoots a heavy explosive bomb in an arc
     * Laser Beam : Shoots a laser beam lol it's just the focalyzer
     * 
     * America Beam : Its a full auto shotgun
     * 
     * Soul Beam : Idk does something cool
     * 
     * Gold Beam : turns enemies into golden statues?
     * 
     * Void Beam: Black vaporises enemies like void jailer
     * Blood Beam: Uses health as ammo, fires spears of solid blood
     * 
     * Flubon Beam: Projectiles bounce 3 - 9 times
     * 
     * Paradox Beam: Speeds up time? LMAO
     */ 
}
