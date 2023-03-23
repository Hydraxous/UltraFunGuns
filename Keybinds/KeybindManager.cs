using System;
using System.Collections;
using UnityEngine;
using UltraFunGuns.Datas;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UltraFunGuns.Keybinds
{
    public static class KeybindManager
    {
        public static DataFile<Keybinds> Keybinds { get; private set; } = new DataFile<Keybinds>(new Keybinds(), "keybinds.txt", Formatting.Indented);

        public static Keybinding Fetch(Keybinding fallback)
        {
            Keybinding binding = fallback;

            StaticCoroutine.RunCoroutine(FetchRoutine(binding));

            return binding;
        } 

        public static void StartKeyRebind(Keybinding bind)
        {
            if(RebindingKey)
            {
                HydraLogger.Log($"Can't start rebinding key {bind.Name}. Another key is being rebound.", DebugChannel.Warning);
                return;
            }

            StaticCoroutine.RunCoroutine(RebindProcess(bind));
        }

        public static bool RebindingKey { get; private set; }
        private static IEnumerator RebindProcess(Keybinding bind)
        {
            float timer = 10.0f;
            RebindingKey = true;
            int counter = 0;

            yield return new WaitForSeconds(0.25f);

            while (RebindingKey && timer > 0.0f)
            {
                yield return null;
                timer -= Time.deltaTime;

                Event current = Event.current;

                if (current.type != EventType.KeyDown && current.type != EventType.KeyUp)
                {
                    continue;
                }

                switch (current.keyCode)
                {
                    case KeyCode.None:
                        break;

                    case KeyCode.Escape:
                        RebindingKey = false;
                        break;
                    default:
                        HydraLogger.Log($"Rebinding to {current.keyCode}", DebugChannel.Warning);
                        bind.SetBind(current.keyCode);
                        RebindingKey = false;
                        continue;
                }

                counter++;
            }
            RebindingKey = false;
            HydraLogger.Log($"Rebinding stopped", DebugChannel.Warning);
        }

        private static IEnumerator FetchRoutine(Keybinding binding)
        {
            Debug.Log($"Getting binding. {binding.Name}:{binding.KeyCode}");

            while (Keybinds == null)
            {
                yield return new WaitForEndOfFrame();
            }

            binding = Keybinds.Data.Bind(binding);
            Debug.Log($"Binding fetched. {binding.Name}:{binding.KeyCode}");
        }

    }

    [System.Serializable]
    public class Keybinds : Validatable
    {
        public List<Keybinding> binds = new List<Keybinding>();

        public bool TryGetBind(string name, out Keybinding binding)
        {
            binding = null;

            foreach (Keybinding bind in binds)
            {
                if (bind.Name == name)
                {
                    binding = bind;
                    return true;
                }
            }

            return false;
        }

        public Keybinding Bind(Keybinding fallback)
        {
            Keybinding cachedBind = fallback;

            foreach (Keybinding bind in binds)
            {
                if (bind.Name == fallback.Name)
                {
                    fallback = bind;
                }
            }

            if (fallback == cachedBind)
            {
                binds.Add(fallback);
                DataManager.SaveAll();
            }

            return fallback;
        }

        public bool BindExists(string name)
        {
            return TryGetBind(name, out Keybinding bind);
        }

        public void SaveBind(Keybinding bind)
        {
            if (!BindExists(bind.Name))
            {
                binds.Add(bind);
            }

            KeybindManager.Keybinds.Save();
        }

        public Keybinds()
        {
            binds = new List<Keybinding>();
        }

        public override bool Validate()
        {
            if (binds == null)
            {
                return false;
            }
            return true;
        }

    }
}
