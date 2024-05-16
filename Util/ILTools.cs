using HarmonyLib;
using System.Reflection.Emit;

namespace UltraFunGuns
{
    public static class ILTools
    {
        public static bool ILOrderMatch(CodeInstruction[] instructions, out int index, int startIndex = 0, params OpCode[] orderMatch)
        {
            index = -1;
            int instructionLength = instructions.Length;
            int orderMatchLength = orderMatch.Length;

            for (int i = startIndex; i < instructions.Length; i++)
            {
                for (int j = 0; j < orderMatch.Length; j++)
                {
                    if (instructions[i + j].opcode != orderMatch[j])
                        break;

                    if (j == orderMatchLength - 1)
                    {
                        index = i;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
