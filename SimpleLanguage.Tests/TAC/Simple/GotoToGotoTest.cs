﻿using NUnit.Framework;
using SimpleLang;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleLanguage.Tests.TAC.Simple
{
    [TestFixture]
    class GotoToGotoTest : TACTestsBase
    {
        [Test]
        public void Test1()
        {
            var TAC = GenTAC(@"
var a, b;
1: goto 2;
2: goto 5;
3: goto 6;
4: a = 1;
5: goto 6;
6: a = b;
");
            ThreeAddressCodeOptimizer.Optimizations.Clear();
            ThreeAddressCodeOptimizer.Optimizations.Add(ThreeAddressCodeGotoToGoto.ReplaceGotoToGoto);

            var expected = new List<string>()
            {
                "1: goto 6",
                "2: goto 6",
                "3: goto 6",
                "4: a = 1",
                "5: goto 6",
                "6: a = b",
            };
            var actual = ThreeAddressCodeOptimizer.Optimize(TAC)
                .Select(instruction => instruction.ToString());

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGotoIfElseTACGen1()
        {
            var TAC = GenTAC(@"
var a,b;
b = 5;
if(a > b)
	goto 6;
6: a = 4;
");
            ThreeAddressCodeOptimizer.Optimizations.Clear();
            ThreeAddressCodeOptimizer.Optimizations.Add(ThreeAddressCodeGotoToGoto.ReplaceGotoToGoto);

            var expected = new List<string>()
            {
                "b = 5",
                "#t1 = a > b",
                "if #t1 goto 6",
                "goto L2",
                "L1: goto 6",
                "L2: noop",
                "6: a = 4",
            };
            var actual = ThreeAddressCodeOptimizer.Optimize(TAC)
                .Select(instruction => instruction.ToString());

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGotoIfElseTACGen2()
        {
            var TAC = GenTAC(@"
var a,b;
b = 5;
if(a > b)
	goto 6;
else
    goto 4;
6: a = 4;
4: a = 6;

");
            ThreeAddressCodeOptimizer.Optimizations.Clear();
            ThreeAddressCodeOptimizer.Optimizations.Add(ThreeAddressCodeGotoToGoto.ReplaceGotoToGoto);

            var expected = new List<string>()
            {
                "b = 5",
                "#t1 = a > b",
                "if #t1 goto 6",
                "goto 4",
                "goto L2",
                "L1: goto 6",
                "L2: noop",
                "6: a = 4",
                "4: a = 6",
            };
            var actual = ThreeAddressCodeOptimizer.Optimize(TAC)
                .Select(instruction => instruction.ToString());

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Test2()
        {
            var TAC = GenTAC(@"
var a, b;
goto 1;
1: if (true) 
    goto 2;
2: a = 5;
");
            ThreeAddressCodeOptimizer.Optimizations.Clear();
            ThreeAddressCodeOptimizer.Optimizations.Add(ThreeAddressCodeGotoToGoto.ReplaceGotoToGoto);

            var expected = new List<string>()
            {
                "goto 2",
                "1: if True goto 2",
                "goto L2",
                "L1: goto 2",
                "L2: noop",
                "2: a = 5",
            };
            var actual = ThreeAddressCodeOptimizer.Optimize(TAC)
                .Select(instruction => instruction.ToString());

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
