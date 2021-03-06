﻿using System;
using System.Collections.Generic;

namespace SimpleLang
{
    public static class ThreeAddressCodeGotoToGoto
    {

        public struct GtotScaner
        {
            public int index;
            public string label;
            public string labelfrom;

            public GtotScaner(int index, string label, string labelfrom)
            {
                this.index = index;
                this.label = label;
                this.labelfrom = labelfrom;
            }
        }
        public static Tuple<bool, List<Instruction>> ReplaceGotoToGoto(List<Instruction> commands)
        {
            bool changed = false;
            List<GtotScaner> list = new List<GtotScaner>();
            List<Instruction> tmpcommands = new List<Instruction>();
            for (int i = 0; i < commands.Count; i++)
            {
                tmpcommands.Add(commands[i]);
                if (commands[i].Operation == "goto")
                {
                    list.Add(new GtotScaner(i, commands[i].Label, commands[i].Argument1));
                }

                if (commands[i].Operation == "ifgoto")
                {
                    list.Add(new GtotScaner(i, commands[i].Label, commands[i].Argument2));
                }
            }

            for (int i = 0; i < tmpcommands.Count; i++)
            {
                if (tmpcommands[i].Operation == "goto")
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].label == tmpcommands[i].Argument1)
                        {
                            changed = true;
                            int index = i >= list.Count ? j - 1 : i;
                            tmpcommands[i] = new Instruction(list[index].label, "goto", list[j].labelfrom.ToString(), "", "");
                        }
                    }
                }

                if (tmpcommands[i].Operation == "ifgoto")
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].label == tmpcommands[i].Argument2)
                        {
                            changed = true;
                            tmpcommands[i] = new Instruction(tmpcommands[i].Label, "ifgoto", tmpcommands[i].Argument1, list[j].labelfrom.ToString(), "");
                        }
                    }
                }
            }
            return Tuple.Create(changed, tmpcommands);

        }
    }
}
