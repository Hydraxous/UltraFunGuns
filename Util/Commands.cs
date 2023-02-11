using System;
using System.Collections.Generic;
using System.Text;
using GameConsole;

namespace UltraFunGuns
{
    public static class Commands
    {
        public static void Register()
        {
            if(!MonoSingleton<GameConsole.Console>.Instance.recognizedCommands.ContainsKey(new UFGCommand().Name.ToLower()))
            {
                MonoSingleton<GameConsole.Console>.Instance.RegisterCommand(new UFGCommand());
            }
        }

        public class UFGCommand : ICommand
        {
            public string Name => "Ultra Fun Guns Command";

            public string Description => "Perform actions in the UltraFunGuns mod.";

            public string Command => "UFG";

            public void Execute(GameConsole.Console con, string[] args)
            {
                if (args.Length > 0)
                {

                }
            }

            private void InterpretCommand(params string[] args)
            {

            }

            private void PrintHelp(GameConsole.Console con)
            {

            }
        }
    }
}
