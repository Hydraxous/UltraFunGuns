using System;
using System.Collections.Generic;
using System.Text;
using HydraDynamics.Debugging;

namespace UltraFunGuns
{
    //Dont care didnt ask plus i just drank water,
    public static class Deboog
    {
        public static void Log(string msg, DebugChannel channel = DebugChannel.Message)
        {
            int newChannel = (int)channel;

            HydraDynamics.Debugging.DebugChannel chan = (HydraDynamics.Debugging.DebugChannel)Enum.ToObject(typeof(HydraDynamics.Debugging.DebugChannel), newChannel);

            UltraFunGuns.UFG.HLogger.Log(msg, chan);
        }
    }

    public enum DebugChannel { User, Message, Warning, Spam, Error, Fatal, Raw }
}
