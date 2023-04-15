using System;
using System.Collections.Generic;
using System.Text;

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


    }
}
