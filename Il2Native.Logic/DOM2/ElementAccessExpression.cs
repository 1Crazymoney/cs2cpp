﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM2
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public class ElementAccessExpression : Expression
    {
        public Expression Index { get; set; }

        public override Kinds Kind
        {
            get { return Kinds.ElementAccessExpression; }
        }

        public Expression Operand { get; set; }

        internal bool Parse(BoundPointerIndirectionOperator boundPointerIndirectionOperator)
        {
            base.Parse(boundPointerIndirectionOperator);

            var boundBinaryOperator = boundPointerIndirectionOperator.Operand as BoundBinaryOperator;
            if (boundBinaryOperator != null)
            {
                this.Operand = Deserialize(boundBinaryOperator.Left) as Expression;
                var boundBinaryOperatorWithIndex = boundBinaryOperator.Right as BoundBinaryOperator;
                if (boundBinaryOperatorWithIndex != null)
                {
                    this.Index = Deserialize(boundBinaryOperatorWithIndex.Left) as Expression;
                }
                else
                {
                    var boundSizeOfOperator = boundBinaryOperator.Right as BoundSizeOfOperator;
                    if (boundSizeOfOperator != null)
                    {
                        this.Index = new Literal { Value = ConstantValue.Create(1) };
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            var conversion = this.Index as Conversion;
            if (conversion != null && conversion.ConversionKind == ConversionKind.IntegerToPointer)
            {
                this.Index = conversion.Operand;
            }

            return true;
        }

        internal override void Visit(Action<Base> visitor)
        {
            this.Operand.Visit(visitor);
            this.Index.Visit(visitor);
            base.Visit(visitor);
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            c.WriteWrappedExpressionIfNeeded(this.Operand);
            c.TextSpan("[");
            this.Index.WriteTo(c);
            c.TextSpan("]");
        }
    }
}
