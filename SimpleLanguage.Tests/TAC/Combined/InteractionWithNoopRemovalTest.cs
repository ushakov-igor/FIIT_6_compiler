using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleLang;
using SimpleLanguage.Tests.TAC.Simple;

namespace SimpleLanguage.Tests.TAC.Combined
{
    [TestFixture]
    public class InteractionWithNoopRemovalTest : TACTestsBase
    {
        public List<Instruction> Optimize(
            List<Instruction> tac,
            List<Func<List<Instruction>, Tuple<bool, List<Instruction>>>> optimizations
        )
        {
            ThreeAddressCodeOptimizer.Optimizations.Clear();
            optimizations.ForEach(opt => 
                ThreeAddressCodeOptimizer.Optimizations.Add(opt)
            );
            return ThreeAddressCodeOptimizer.Optimize(tac);
        }

        public void AssertEquality(List<Instruction> result, List<string> expected)
        {
            var resultStr = result.Select(x => x.ToString());
            CollectionAssert.AreEqual(expected, resultStr);
        }

        [Test]
        public void RemoveEmptyNodesShouldCleanUpAfterDeleteDeadCodeWithDeadVars()
        {
            var TAC = GenTAC(
            @"
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
            var opts = new List<Func<List<Instruction>, Tuple<bool, List<Instruction>>>>()
            {
                DeleteDeadCodeWithDeadVars.DeleteDeadCode,
                ThreeAddressCodeRemoveNoop.RemoveEmptyNodes,
            };
            
            var result = Optimize(TAC, opts);
            var expected = new List<String>
            {
                "b = 22",
                "c = 1",
                "#t1 = b + c",
                "a = #t1",
                "c = 1",
                "#t3 = a - c",
                "b = #t3",
                "#t4 = -b",
                "a = #t4",
            };
            
            AssertEquality(result, expected);   
        }

        [Test]
        public void RemoveEmptyNodesShouldNotDeleteNoopAfterLastCycle()
        {
            var TAC = GenTAC(
                @"
                var a, i;
                for i = 1,5
                    a = 10;
                ");
            var opts = new List<Func<List<Instruction>, Tuple<bool, List<Instruction>>>>()
            {
                ThreeAddressCodeRemoveNoop.RemoveEmptyNodes,
            };

            var result = Optimize(TAC, opts);
            Assert.AreEqual(result[result.Count - 1].Operation, "noop");
        }

        [Test]
        public void RemoveEmptyNodesShouldWorkWithGotoToGoto()
        {
            var TAC = new List<Instruction>
            {
                new Instruction("1", "goto", "5", null, null),
                new Instruction("2", "goto", "5", null, null),
                new Instruction("", "noop", null, null, null),
                new Instruction("5", "assign", "b", null, "a"),
            };
            
            var opts = new List<Func<List<Instruction>, Tuple<bool, List<Instruction>>>>()
            {
                ThreeAddressCodeGotoToGoto.ReplaceGotoToGoto,
                ThreeAddressCodeRemoveNoop.RemoveEmptyNodes
            };

            var result = Optimize(TAC, opts);

            var expected = new List<String>
            {
                "1: goto 5",
                "2: goto 5",
                "5: a = b",
            };
            
            AssertEquality(result, expected);
        }

        [Test]
        public void RemoveEmptyNodesWorksWithRemoveGotoThroughGoto()
        {
            var TAC = GenTAC(@"
var a;
1:  if (1 < 2) 
        a = 4 + 5 * 6;
    else
        goto 4;");
            
            var opts = new List<Func<List<Instruction>, Tuple<bool, List<Instruction>>>>()
            {
                ThreeAddressCodeRemoveGotoThroughGoto.RemoveGotoThroughGoto,
                ThreeAddressCodeRemoveNoop.RemoveEmptyNodes
            };

            var result = Optimize(TAC, opts);

            var expected = new List<string>
            {
                "1: #t1 = 1 < 2",
                "if #t1 goto L1",
                "goto 4",
                "goto L2",
                "L1: #t2 = 5 * 6",
                "#t3 = 4 + #t2",
                "a = #t3",
                "L2: noop"
            };
            
            AssertEquality(result, expected);
        }
    }
}