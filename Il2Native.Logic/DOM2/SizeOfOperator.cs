﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM2
{
    using System;
    using Microsoft.CodeAnalysis.CSharp;

    public class SizeOfOperator : Expression
    {
        public override Kinds Kind
        {
            get { return Kinds.SizeOfOperator; }
        }

        public Expression SourceType { get; set; }

        internal void Parse(BoundSizeOfOperator boundSizeOfOperator)
        {
            base.Parse(boundSizeOfOperator);
            this.SourceType = Deserialize(boundSizeOfOperator.SourceType) as Expression;
        }

        internal override void Visit(Action<Base> visitor)
        {
            this.SourceType.Visit(visitor);
            base.Visit(visitor);
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            c.TextSpan("sizeof(");
            this.SourceType.WriteTo(c);
            c.TextSpan(")");
        }
    }
}
