using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;
using NewBlood;

namespace UltraFunGuns
{
    public class UFGBind
    {
        public KeyCode KeyCode { get; private set; }
        public string Name { get; private set; }

        public UFGBind(string name, KeyCode bind)
        {
            KeyCode = bind;
            Name = name;
        }

        public void SetBind(KeyCode bind)
        {
            KeyCode = bind;
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
