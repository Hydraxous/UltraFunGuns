using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;
using NewBlood;
using UltraFunGuns.Keybinds;
using UltraFunGuns.Datas;
using Newtonsoft.Json;

namespace UltraFunGuns
{
    [Serializable]
    public class Keybinding
    {
        public KeyCode KeyCode { get; private set; }
        public string Name { get; private set; }

        public Keybinding(string name, KeyCode bind)
        {
            Name = name;
            KeyCode = bind;
        }

        public void Rebind()
        {
            KeybindManager.StartKeyRebind(this);
        }

        public void SetBind(KeyCode bind)
        {
            KeyCode = bind;
            Save();
        }

        //this sucks so bad T-T
        public void Save()
        {
            KeybindManager.Keybinds.Data.SaveBind(this);
        }

        [JsonIgnore]
        public bool IsPressed
        {
            get
            {
                return Input.GetKey(KeyCode);
            }
        }

        [JsonIgnore]
        public bool WasPerformedThisFrame
        {
            get
            {
                return Input.GetKeyDown(KeyCode);
            }
        }
    }
}
