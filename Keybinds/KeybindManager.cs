using System;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns.Keybinds
{
    public static class KeybindManager
    {
        public static UFGBind Fetch(UFGBind fallback)
        {
            UFGBind binding = fallback;

            return fallback;

            if(TryGetBindFromFile(fallback.Name, out UFGBind boundKey))
            {
                binding = boundKey;
            }else
            {
                Data.Keybinds.Data.binds.Add(binding);
                Data.Keybinds.Save();
            }

            return binding;
        }

        private static bool TryGetBindFromFile(string name, out UFGBind binding)
        {
            binding = null;

            try
            {
                if (!Data.Keybinds.Data.TryGetBind(name, out binding))
                {
                    return false;
                }

                return true;
            }
            catch(Exception e)
            {
                HydraLogger.Log($"{e.Message}\n\n{e.StackTrace}", DebugChannel.Warning);
            }

            return false;
            
        }

        

        public static void StartKeyRebind(UFGBind bind)
        {
            if(RebindingKey)
            {
                return;
            }
        }

        public static bool RebindingKey { get; private set; }
        private static IEnumerator RebindProcess(UFGBind bind)
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

    }
}
