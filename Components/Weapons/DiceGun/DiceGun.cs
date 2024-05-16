using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace UltraFunGuns
{
    [UFGWeapon("dice_gun", "Deterministic Observer", 2, false, WeaponIconColor.Blue, false)]
    public class DiceGun : UltraFunGunBase
    {
        [Configgy.Configgable("Weapons/Deterministic Observer/Deterministic Observer")]
        private static int startingDice = 3;

        [Configgy.Configgable("Weapons/Deterministic Observer/Deterministic Observer")]
        private static int maxDice = 12;

        [Configgy.Configgable("Weapons/Deterministic Observer/Deterministic Observer")]
        private static float diceSpawnFloorOffset = 4f;

        [Configgy.Configgable("Weapons/Deterministic Observer/Deterministic Observer")]
        private static int diceDropCount = 1;

        [Configgy.Configgable("Weapons/Deterministic Observer/Deterministic Observer", displayName:"debug_all_spells")]
        private static bool addSpellsOnEnable = false;

        private int diceLeft;
        private bool primaryPressedThisFrame => InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && !om.paused;
        private bool secondaryPressedThisFrame => InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && !om.paused;

        private float timeUntilAction = 0f;

        private List<DiceGunSpell> currentSpellPool = new List<DiceGunSpell>();

        private DiceGunSpell chamberedSpell;

        private static readonly DiceGunSpell[] allSpells = new DiceGunSpell[]
        {
            new DisintigrateSpell()
        };

        public override void OnAwakeFinished()
        {
            diceLeft = startingDice;
        }

        public Transform CameraTransform => mainCam;
        public Transform FirePoint => firePoint;

        private void Update()
        {
            HandlePrimaryAction();
        }

        private void HandlePrimaryAction()
        {
            if (timeUntilAction > 0) //cooldown
                return;

            if (primaryPressedThisFrame && chamberedSpell != null)
            {
                Fire();
                return;
            }

            if (secondaryPressedThisFrame)
            {
                RollDice();
                return;
            }
        }

        private void Fire()
        {
            if (chamberedSpell == null)
                return;

            chamberedSpell.ExecuteSpell(this);
            
            DiceGunSpell lastSpell = chamberedSpell;
            chamberedSpell = null;

            lastSpell?.OnSpellDiscarded();
        }

        private void RollDice()
        {
            //No spells picked up
            currentSpellPool = currentSpellPool.Where(x => x != null).ToList();
            if (currentSpellPool.Count <= 0)
                return;

            DiceGunSpell lastSpell = chamberedSpell;
            chamberedSpell = currentSpellPool.RandomEntry();
            currentSpellPool.Remove(chamberedSpell);
            chamberedSpell.OnSpellRolled(UnityEngine.Random.Range(0,20));

            lastSpell?.OnSpellDiscarded();
        }

        public bool CanPickupSpells()
        {
            return currentSpellPool.Count < maxDice;
        }

        public void PickupRandomSpell(DiceGunSpell spell)
        {
            currentSpellPool.Add(spell);
            spell.OnSpellAddedToPool(this);
        }

        [Configgy.Configgable("Weapons/Deterministic Observer/Deterministic Observer", displayName:"Add random spell to pool")]
        public static void PickupRandomSpellStatic()
        {
            //Debug
            GameObject.FindObjectOfType<DiceGun>()?.PickupRandomSpell();
        }

        public void PickupRandomSpell()
        {
            PickupRandomSpell(allSpells.RandomEntry());
        }

        private void OnEnable()
        {
            UKEvents.Enemy.OnEnemyDeath += Enemy_OnEnemyDeath;

            if(addSpellsOnEnable)
            {
                currentSpellPool.Clear();

                for(int i= 0;i < allSpells.Length;i++)
                {
                    PickupRandomSpell(allSpells[i]);
                }
            }
        }

        private void Enemy_OnEnemyDeath(UKEvents.Enemy.EnemyDeathEvent enemyDeath)
        {
            if (enemyDeath.LastHit.IsValid())
                return;

            SpawnDice(enemyDeath.EnemyIdentifier.transform.position);
        }

        private void SpawnDice(Vector3 position)
        {
            
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                position = hit.position + Vector3.up * diceSpawnFloorOffset;
            }

            for(int i=0;i<diceDropCount;i++)
            {
                
                //Instantiate dice pickup.
                //GameObject.Instantiate(diceDropPrefab, position, Quaternion.identity);
            }
        }

        private void OnDisable()
        {
            UKEvents.Enemy.OnEnemyDeath -= Enemy_OnEnemyDeath;
        }
    }
}
