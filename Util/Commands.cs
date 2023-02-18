using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using GameConsole;

namespace UltraFunGuns
{
    public static class Commands
    {
        public static string CommandAlias => "ufg";

        private static Dictionary<string, Dictionary<string,UFGDebugMethod>> ufgCommands = new Dictionary<string, Dictionary<string, UFGDebugMethod>>();

        public static void Register()
        {
            RegisterCommands();

            if(!MonoSingleton<GameConsole.Console>.Instance.recognizedCommands.ContainsKey(new UFGCommand().Name.ToLower()))
            {
                MonoSingleton<GameConsole.Console>.Instance.RegisterCommand(new UFGCommand());
            }
        }

        private static void RegisterCommands()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                MethodInfo[] methods = type.GetMethods();
                
                if(methods.Length > 0)
                {
                    foreach (MethodInfo method in methods)
                    {
                        if(method.IsStatic)
                        {
                            UFGDebugMethod newCommand = method.GetCustomAttribute<UFGDebugMethod>();

                            if (newCommand != null)
                            {
                                
                                string typeName = type.Name.ToLower();
                                string methodName = method.Name.ToLower();

                                if (!ufgCommands.ContainsKey(typeName))
                                {
                                    ufgCommands.Add(typeName, new Dictionary<string, UFGDebugMethod>());
                                }

                                if (!ufgCommands[typeName].ContainsKey(methodName))
                                {
                                    bool reg = true;

                                    ParameterInfo[] parameters = method.GetParameters();
                                    for (int i = 0; i < parameters.Length; i++)
                                    {
                                        if (!parameters[i].HasDefaultValue)
                                        {
                                            reg = false;
                                        }
                                    }

                                    if (reg)
                                    {
                                        object[] defaultParams = parameters.Select(x => x.DefaultValue).ToArray();
                                        newCommand.Setup(() => { method.Invoke(null, defaultParams); }, methodName);
                                        ufgCommands[typeName].Add(methodName, newCommand);
                                    }   
                                }
                            }
                        }  
                    }
                }
            }
        }

        public class UFGCommand : ICommand
        {
            public string Name => "Ultra Fun Guns Command";

            public string Description => "Perform actions in the UltraFunGuns mod.";

            public string Command => "ufg";

            //TODO clean this up.
            public void Execute(GameConsole.Console con, string[] args)
            {
                if (args.Length > 0)
                {
                    if (ufgCommands.ContainsKey(args[0].ToLower()))
                    {
                        if(args.Length > 1)
                        {
                            if (ufgCommands[args[0].ToLower()].ContainsKey(args[1].ToLower()))
                            {
                                ufgCommands[args[0].ToLower()][args[1].ToLower()].Action?.Invoke();
                            }
                            else
                            {
                                con.PrintLine($"UFG {args[0].ToLower()} method does not exist.", ConsoleLogType.Error);
                            }
                        }else
                        {
                            foreach(KeyValuePair<string, UFGDebugMethod> keyValuePair in ufgCommands[args[0].ToLower()])
                            {
                                con.PrintLine($"{Commands.CommandAlias} {args[0].ToLower()} {keyValuePair.Value.MethodName} - {keyValuePair.Value.Name}: {keyValuePair.Value.Description}");
                            }
                        }
                    }
                    else
                    {
                        con.PrintLine("UFG Subcommand does not exist.", ConsoleLogType.Error);
                    }
                }
                else
                {
                    PrintHelp(con);
                }
            }
            private void PrintHelp(GameConsole.Console con)
            {
                foreach (KeyValuePair<string, Dictionary<string, UFGDebugMethod>> keyValuePair in ufgCommands)
                {
                    con.PrintLine($"{Commands.CommandAlias} {keyValuePair.Key}");
                }
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class UFGDebugMethod : Attribute
        {
            public string Name { get; }
            public string Description { get; }

            public string MethodName { get; private set; }

            public Action Action { get; private set; }

            public UFGDebugMethod(string Name, string Description)
            {
                this.Name = Name;
                this.Description = Description;
            }

            public void Setup(Action method, string methodName)
            {
                Action = method;
                MethodName = methodName;
            }

            public void Execute()
            {
                Action?.Invoke();
            }
        }
    }
}
