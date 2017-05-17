﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM2
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public class LambdaExpression : Expression
    {
        private readonly IList<ILocalSymbol> _locals = new List<ILocalSymbol>();

        public override Kinds Kind
        {
            get { return Kinds.LambdaExpression; }
        }

        public IList<ILocalSymbol> Locals
        {
            get { return this._locals; }
        }

        public Base Statements
        {
            get; set;
        }

        internal void Parse(BoundStatementList boundStatementList)
        {
            var block = new Block();
            block.Parse(boundStatementList);
            this.Statements = block;
        }

        internal override void Visit(Action<Base> visitor)
        {
            this.Statements.Visit(visitor);
            base.Visit(visitor);
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            c.TextSpan("[&]");
            c.TextSpan("(");

            var any = false;
            foreach (var local in this.Locals)
            {
                if (any)
                {
                    c.TextSpan(",");
                    c.WhiteSpace();
                }

                c.TextSpan("auto");
                c.WhiteSpace();
                c.WriteName(local);

                any = true;
            }

            c.TextSpan(")");
            c.WriteBlockOrStatementsAsBlock(this.Statements, true);
        }
    }
}
