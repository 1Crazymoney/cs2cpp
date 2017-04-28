﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM2
{
    using System;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis.CSharp;

    public class WhileStatement : BlockStatement
    {
        public Expression Condition { get; set; }

        public override Kinds Kind
        {
            get { return Kinds.WhileStatement; }
        }

        internal bool Parse(BoundStatementList boundStatementList)
        {
            if (boundStatementList == null)
            {
                throw new ArgumentNullException();
            }

            var stage = Stages.Initialization;
            foreach (var boundStatement in IterateBoundStatementsList(boundStatementList))
            {
                var boundLabelStatement = boundStatement as BoundLabelStatement;
                if (boundLabelStatement != null)
                {
                    var labelSymbol = boundLabelStatement.Label;
                    if (labelSymbol.NeedsLabel("start") && stage == Stages.Initialization)
                    {
                        stage = Stages.Body;
                        continue;
                    }

                    if (labelSymbol.NeedsLabel("continue") && stage == Stages.Body)
                    {
                        stage = Stages.Condition;
                        continue;
                    }

                    if (labelSymbol.NeedsLabel("break") && stage == Stages.Condition)
                    {
                        stage = Stages.End;
                        continue;
                    }
                }

                if (stage == Stages.Initialization)
                {
                    var boundGotoStatement = boundStatement as BoundGotoStatement;
                    if (boundGotoStatement != null && boundGotoStatement.Label.NeedsLabel("continue"))
                    {
                        continue;
                    }

                    return false;
                }

                if (stage == Stages.Condition)
                {
                    var boundConditionalGoto = boundStatement as BoundConditionalGoto;
                    if (boundConditionalGoto != null)
                    {
                        this.Condition = Deserialize(boundConditionalGoto.Condition) as Expression;
                        Debug.Assert(this.Condition != null);
                        continue;
                    }
                }

                var statement = Deserialize(boundStatement);
                if (statement != null)
                {
                    switch (stage)
                    {
                        case Stages.Body:
                            Debug.Assert(Statements == null);
                            Statements = statement;
                            break;
                        default:
                            return false;
                    }
                }
            }

            return stage == Stages.End;
        }

        internal override void Visit(Action<Base> visitor)
        {
            this.Condition.Visit(visitor);
            base.Visit(visitor);
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            c.TextSpan("while");
            c.WhiteSpace();
            c.TextSpan("(");

            this.Condition.WriteTo(c);

            c.TextSpan(")");

            c.NewLine();
            base.WriteTo(c);
        }

        private enum Stages
        {
            Initialization,
            Body,
            Condition,
            End
        }
    }
}
