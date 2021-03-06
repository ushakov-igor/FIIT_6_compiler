﻿using NUnit.Framework;
using SimpleLang;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleLanguage.Tests.TAC.Simple
{
    [TestFixture]
    class DeleteDeadCodeWithDeadVarsTest : TACTestsBase
    {
        [Test]
        public void Test()
        {
            var TAC = GenTAC(@"
var a, b, c;
a = 1;
a = 2;
b = 11;
b = 22;
a = 3;
a = b;
c = 1;
a = b + c;
b = -c;
c = 1;
b = a - c;
a = -b;
");
            ThreeAddressCodeOptimizer.Optimizations.Clear();
            ThreeAddressCodeOptimizer.Optimizations.Add(DeleteDeadCodeWithDeadVars.DeleteDeadCode);

            var expected = new List<string>()
            {
                "noop",
                "noop",
                "noop",
                "b = 22",
                "noop",
                "noop",
                "c = 1",
                "#t1 = b + c",
                "a = #t1",
                "noop",
                "noop",
                "c = 1",
                "#t3 = a - c",
                "b = #t3",
                "#t4 = -b",
                "a = #t4",
            };
            var actual = ThreeAddressCodeOptimizer.Optimize(TAC)
                .Select(instruction => instruction.ToString());

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void TempVars()
        {
            var TAC = GenTAC(@"
var a;
a = -a;
a = 1;
");
            ThreeAddressCodeOptimizer.Optimizations.Clear();
            ThreeAddressCodeOptimizer.Optimizations.Add(DeleteDeadCodeWithDeadVars.DeleteDeadCode);

            var expected = new List<string>()
            {
                "noop",
                "noop",
                "a = 1"
            };
            var actual = ThreeAddressCodeOptimizer.Optimize(TAC)
                .Select(instruction => instruction.ToString());

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Negation()
        {
            var TAC = GenTAC(@"
var a;
a = true;
a = !a;
");
            ThreeAddressCodeOptimizer.Optimizations.Clear();
            ThreeAddressCodeOptimizer.Optimizations.Add(DeleteDeadCodeWithDeadVars.DeleteDeadCode);

            var expected = new List<string>()
            {
                "a = True",
                "#t1 = !a",
                "a = #t1"
            };
            var actual = ThreeAddressCodeOptimizer.Optimize(TAC)
                .Select(instruction => instruction.ToString());

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
