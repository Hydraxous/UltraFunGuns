using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UltraFunGuns.Datas
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UFGPersistent : Attribute
    {
        public string FileName { get; }
        public string Group { get; }
        public Type Type { get; private set; }

        public UFGPersistent(string fileName = "config", string group = "general")
        {
            FileName = fileName;
            Group = group;
        }

        public void SetType(Type type)
        {
            Type = type;
        }
    }

    public class ExampleClass
    {
        private static Persistable<bool> isFunny = new(true, "config", "exmaple", "Funny Boolean") { Value = true };
        private static Persistable<int> funnyValue = new(0,"config","example","Funny Value") { Value = 1 };


        public void SetStuff(bool val1, int val2)
        {
            isFunny.Value = val1;
            funnyValue.Value = val2;
        }

        public void DoThing()
        {
            if(isFunny)
            {
                Deboog.Log("It do be funny.", DebugChannel.Warning);
            }else
            {
                Deboog.Log("It dont be funny.", DebugChannel.Warning);
            }

            if (funnyValue > 0)
            {
                Deboog.Log("It do be over 0", DebugChannel.Warning);
            }else
            {
                Deboog.Log("It dont be over 0", DebugChannel.Warning);
            }
        }
    }

    public class Persistable<T>
    {
        private T defaultValue;
        private T value;
        private bool autosave = true;
        private string fileName, displayName, group;

        public T Value
        {
            get
            {
                if (value == null)
                {
                    value = PersistenceManager.LoadPersistable<T>();
                }
                return value;
            }

            set
            {
                this.value = value;
                if (autosave)
                {
                    Save();
                }
            }
        }

        public Persistable(T defaultValue, string fileName, string group, string displayName)
        {
            this.defaultValue = defaultValue;
            this.fileName = fileName;
            this.group = group;
            this.displayName = displayName;
        }

        public void Save()
        {
            Deboog.Log($"\"Saved\": {value}", DebugChannel.Warning);
        }

        public void Load()
        {
            if(value == null)
            {
                value = default(T);
            }
            Deboog.Log($"\"Loaded\": {value}", DebugChannel.Warning);
        }

        public void New()
        {
            value = defaultValue;
        }

        /*
        public static implicit operator bool (Persistable<T> value)
        {
            bool boolean;

            if(typeof(T).Equals(typeof(bool)))
            {
                boolean = (bool)(object)value.Value;
                return boolean;
            }

            return value != null;
        }
        */

        public static implicit operator T (Persistable<T> value)
        {

            if (typeof(T).Equals(typeof(T)))
            {
                return (T)(object)value.Value;
            }

            throw new InvalidOperationException("You tried to operate on a thing if it were another one.");
        }


        public override string ToString()
        {
            return Value.ToString();
        }

    }
}
