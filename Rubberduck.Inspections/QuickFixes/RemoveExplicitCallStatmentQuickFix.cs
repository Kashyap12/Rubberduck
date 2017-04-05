using Antlr4.Runtime;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Inspections.Resources;
using Rubberduck.VBEditor;

namespace Rubberduck.Inspections.QuickFixes
{
    public class RemoveExplicitCallStatmentQuickFix : IQuickFix
    {
        public RemoveExplicitCallStatmentQuickFix(ParserRuleContext context, QualifiedSelection selection)
            : base(context, selection, InspectionsUI.RemoveObsoleteStatementQuickFix)
        {
        }

        public void Fix(IInspectionResult result)
        {
            var module = Selection.QualifiedName.Component.CodeModule;

            var selection = Context.GetSelection();
            var originalCodeLines = module.GetLines(selection.StartLine, selection.LineCount);
            var originalInstruction = Context.GetText();

            var context = (VBAParser.CallStmtContext)Context;

            string target;
            string arguments;
            // The CALL statement only has arguments if it's an index expression.
            if (context.expression() is VBAParser.LExprContext && ((VBAParser.LExprContext)context.expression()).lExpression() is VBAParser.IndexExprContext)
            {
                var indexExpr = (VBAParser.IndexExprContext)((VBAParser.LExprContext)context.expression()).lExpression();
                target = indexExpr.lExpression().GetText();
                arguments = " " + indexExpr.argumentList().GetText();
            }
            else
            {
                target = context.expression().GetText();
                arguments = string.Empty;
            }
            module.DeleteLines(selection.StartLine, selection.LineCount);
            var newInstruction = target + arguments;
            var newCodeLines = originalCodeLines.Replace(originalInstruction, newInstruction);

            module.InsertLines(selection.StartLine, newCodeLines);
        }
    }
}
