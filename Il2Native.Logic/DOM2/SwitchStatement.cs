﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DOM.Implementations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Symbols;

    public class SwitchStatement : Statement
    {
        private Expression expression;
        private readonly IList<Statement> statements = new List<Statement>();
        private MethodSymbol stringEquality;
        private readonly IList<SwitchSection> switchCases = new List<SwitchSection>();

        public override Kinds Kind
        {
            get { return Kinds.SwitchStatement; }
        }

        internal void Parse(BoundSwitchStatement boundSwitchStatement)
        {
            if (boundSwitchStatement == null)
            {
                throw new ArgumentNullException();
            }

            /*
            if (boundSwitchStatement.OuterLocals != null)
            {
                AddLocals(boundSwitchStatement.OuterLocals, this.statements);
            }
            */

            if (boundSwitchStatement.InnerLocals != null)
            {
                AddLocals(boundSwitchStatement.InnerLocals, this.statements);
            }

            this.expression = Deserialize(boundSwitchStatement.Expression) as Expression;
            foreach (var boundSwitchSection in boundSwitchStatement.SwitchSections)
            {
                var switchSection = new SwitchSection();
                switchSection.SwitchType = this.expression.Type;
                switchSection.Parse(boundSwitchSection);
                this.switchCases.Add(switchSection);
            }

            this.stringEquality = boundSwitchStatement.StringEquality;

            // disable all 'auto' variables
            this.Visit(
                (e) =>
                    {
                        var assignmentOperator = e as AssignmentOperator;
                        if (assignmentOperator != null)
                        {
                            var local = assignmentOperator.Left as Local;
                            if (local != null && boundSwitchStatement.InnerLocals.Any(ol => ol.Name == local.Name))
                            {
                                assignmentOperator.TypeDeclaration = false;
                            }
                        }
                    });
        }

        internal override void Visit(Action<Base> visitor)
        {
            base.Visit(visitor);
            this.expression.Visit(visitor);
            foreach (var statement in this.statements)
            {
                statement.Visit(visitor);
            }

            foreach (var statement in this.switchCases)
            {
                statement.Visit(visitor);
            }
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            Local localCase = null;
            if (this.stringEquality != null)
            {
                c.OpenBlock();

                var localImpl = new LocalImpl { Name = "__SwitchExpression", Type = this.expression.Type };
                var local = new Local { LocalSymbol = localImpl };

                var localImplCase = new LocalImpl { Name = "__SwitchCase", Type = new TypeImpl { SpecialType = SpecialType.System_Int32 } };
                localCase = new Local { LocalSymbol = localImplCase };

                new VariableDeclaration
                {
                    Statements =
                    {
                        new ExpressionStatement
                        {
                            Expression =
                                new AssignmentOperator
                                {
                                    TypeDeclaration = true,
                                    Type = new TypeImpl { SpecialType = SpecialType.System_Int32 },
                                    Left = localCase,
                                    Right = new Literal { Value = ConstantValue.Create(0) }
                                }
                        }
                    }
                }.WriteTo(c);
                new VariableDeclaration
                {
                    Statements =
                    {
                        new ExpressionStatement
                        {
                            Expression =
                                new AssignmentOperator
                                {
                                    TypeDeclaration = true,
                                    Type = new TypeImpl { SpecialType = SpecialType.System_String },
                                    Left = local,
                                    Right = this.expression
                                }
                        }
                    }
                }.WriteTo(c);

                // first if
                IfStatement first = null;
                IfStatement last = null;
                var caseIndex = 0;

                foreach (var switchSection in this.switchCases)
                {
                    foreach (var label in switchSection.Labels)
                    {
                        if (label.Value == null)
                        {
                            // default case;
                            continue;
                        }

                        caseIndex++;

                        // compare
                        var callEqual = new Call() { Method = this.stringEquality };
                        callEqual.Arguments.Add(local);
                        callEqual.Arguments.Add(new Literal { Value = label.Value });

                        // set value
                        var setExpr = new ExpressionStatement
                                          {
                                              Expression =
                                                  new AssignmentOperator
                                                      {
                                                          Left = localCase,
                                                          Right = new Literal { Value = ConstantValue.Create(caseIndex) }
                                                      }
                                          };

                        var ifStatement = new IfStatement
                                              {
                                                  Condition = callEqual,
                                                  IfStatements = setExpr
                                              };

                        first = first ?? ifStatement;
                        if (last != null)
                        {
                            last.ElseStatementsOpt = ifStatement;
                        }

                        last = ifStatement;

                        // remap case value
                        label.Value = ConstantValue.Create(caseIndex);
                    }
                }

                if (first != null)
                {
                    first.WriteTo(c);
                }

                c.Separate();
            }

            foreach (var statement in this.statements)
            {
                statement.WriteTo(c);
            }

            c.TextSpan("switch");
            c.WhiteSpace();
            c.TextSpan("(");
            if (this.stringEquality != null)
            {
                localCase.WriteTo(c);
            }
            else
            {
                this.expression.WriteTo(c);
            }

            c.TextSpan(")");
            c.NewLine();
            c.OpenBlock();
            foreach (var switchSection in this.switchCases)
            {
                switchSection.WriteTo(c);
            }

            c.EndBlock();

            if (this.stringEquality != null)
            {
                c.EndBlock();
            }

            c.Separate();
        }
    }
}
