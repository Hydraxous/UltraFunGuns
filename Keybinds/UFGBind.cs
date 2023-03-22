using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;
using NewBlood;
using UltraFunGuns.Keybinds;

namespace UltraFunGuns
{
    [Serializable]
    public class UFGBind 
    {
        public KeyCode KeyCode { get; private set; }
        public string Name { get; private set; }

        public UFGBind(string name, KeyCode bind)
        {
            Name = name;
            KeyCode = bind;
        }

        public void SetBind(KeyCode bind)
        {
            KeyCode = bind;
            this.Save();
        }

        public void Save()
        {
            if(!Data.Keybinds.Data.BindExists(Name))
            {
                Data.Keybinds.Data.binds.Add(this);
            }

            Data.Keybinds.Save();
        }

        public bool IsPressed
        {
            get
            {
                return Input.GetKey(KeyCode);
            }
        }


        public bool WasPerformedThisFrame
        {
            get
            {
                return Input.GetKeyDown(KeyCode);
            }
        }

        public bool PlayerHeld
        {
            get
            {
                return IsPressed && !gamePaused;

            }
        }

        public bool PlayerPerformed
        {
            get
            {
                return WasPerformedThisFrame && !gamePaused;
            }
        }

        private static bool gamePaused => OptionsManager.Instance.paused;
    }
}
