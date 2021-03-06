using ProgramTree;
using System;

namespace SimpleLang.Visitors
{
    class OptExprSubEqualVar : ChangeVisitor
    {
        public override void PostVisit(Node n)
        {
            // a - a => 0
            if (n is BinOpNode binop && binop.Op == OpType.MINUS 
                && binop.Left is IdNode id1 && binop.Right is IdNode id2){
                if (id1.Name == id2.Name)
                { 
                    ReplaceExpr(binop, new IntNumNode(0));
                }
                else
                {
                    base.VisitBinOpNode(binop);
                }
            }
        }
    }
}