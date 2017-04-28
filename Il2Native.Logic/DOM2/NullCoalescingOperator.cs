﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM2
{
    using System;
    using DOM.Implementations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public class NullCoalescingOperator : Expression
    {
        public override Kinds Kind
        {
            get { return Kinds.NullCoalescingOperator; }
        }

        public Expression LeftOperand { get; set; }

        public Expression RightOperand { get; set; }

        internal ConversionKind ConversionKind { get; set; }

        internal void Parse(BoundNullCoalescingOperator boundNullCoalescingOperator)
        {
            base.Parse(boundNullCoalescingOperator);
            this.LeftOperand = Deserialize(boundNullCoalescingOperator.LeftOperand) as Expression;
            this.RightOperand = Deserialize(boundNullCoalescingOperator.RightOperand) as Expression;
            this.ConversionKind = boundNullCoalescingOperator.LeftConversion.Kind;
        }

        internal override void Visit(Action<Base> visitor)
        {
            this.LeftOperand.Visit(visitor);
            this.RightOperand.Visit(visitor);
            base.Visit(visitor);
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            // Finish it properly

            var localImpl = new LocalImpl { Name = "__NullCoalescing", Type = Type };
            var local = new Local { LocalSymbol = localImpl };

            var block = new Block();
            block.Statements.Add(new VariableDeclaration { Local = local });
            block.Statements.Add(
                new ExpressionStatement
                {
                    Expression =
                        new AssignmentOperator
                        {
                            Left = local,
                            Right = this.LeftOperand
                        }
                });
            block.Statements.Add(
                new ReturnStatement
                {
                    ExpressionOpt =
                        new ConditionalOperator
                        {
                            Condition =
                                new BinaryOperator
                                {
                                    Left = local,
                                    Right = new Literal { Value = ConstantValue.Create(null) },
                                    OperatorKind = BinaryOperatorKind.NotEqual
                                }, Consequence = local, Alternative = this.RightOperand

                        }
                });
            new LambdaCall
            {
                Lambda = new LambdaExpression { Statements = block, Type = Type }
            }.WriteTo(c);
        }
    }
}
