using System;

namespace UltraFunGuns
{
    public static class Events
    {
        public delegate void EventHandler();

        /// <summary>
        /// Invoked when the player dies.
        /// </summary>
        public static EventHandler OnPlayerDeath;

        /// <summary>
        /// Invoked when the player respawns.
        /// </summary>
        public static EventHandler OnPlayerRespawn;


        public static event Action<NewMovement, int> OnPlayerHurt;

        public static void PlayerHurt(NewMovement player, int damage)
        {
            OnPlayerHurt?.Invoke(player, damage);
        }


    }
}
