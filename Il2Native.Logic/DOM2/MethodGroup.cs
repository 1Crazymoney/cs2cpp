﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public class MethodGroup : Expression
    {
        public Expression InstanceOpt { get; set; }

        public override Kinds Kind
        {
            get { return Kinds.MethodGroup; }
        }

        public IMethodSymbol Method { get; set; }

        public Expression ReceiverOpt { get; set; }

        public IList<ITypeSymbol> TypeArgumentsOpt { get; set; }

        internal void Parse(BoundMethodGroup boundMethodGroup)
        {
            base.Parse(boundMethodGroup);
            if (boundMethodGroup.LookupSymbolOpt != null)
            {
                this.Method = boundMethodGroup.LookupSymbolOpt as IMethodSymbol;
            }

            if (boundMethodGroup.TypeArgumentsOpt != null)
            {
                this.TypeArgumentsOpt = boundMethodGroup.TypeArgumentsOpt.OfType<ITypeSymbol>().ToList();
            }

            if (boundMethodGroup.ReceiverOpt != null)
            {
                this.ReceiverOpt = Deserialize(boundMethodGroup.ReceiverOpt) as Expression;
            }

            if (boundMethodGroup.InstanceOpt != null)
            {
                this.InstanceOpt = Deserialize(boundMethodGroup.InstanceOpt) as Expression;
            }
        }

        internal override void Visit(Action<Base> visitor)
        {
            if (this.InstanceOpt != null)
            {
                this.InstanceOpt.Visit(visitor);
            }

            if (this.ReceiverOpt != null)
            {
                this.ReceiverOpt.Visit(visitor);
            }

            base.Visit(visitor);
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            if (!this.Method.IsStatic)
            {
                var receiverOpt = this.ReceiverOpt;
                if (receiverOpt is BaseReference)
                {
                    receiverOpt = new ThisReference();
                }

                c.WriteWrappedExpressionIfNeeded(receiverOpt);
                c.TextSpan(",");
                c.WhiteSpace();
            }

            new Parenthesis { Operand = new MethodPointer { Method = this.Method } }.WriteTo(c);
            c.TextSpan("&");
            c.WriteTypeFullName(this.Method.ContainingType);
            c.TextSpan("::");
            c.WriteMethodNameNoTemplate(this.Method);

            if (this.TypeArgumentsOpt != null && this.TypeArgumentsOpt.Any())
            {
                c.WriteTypeArguments(this.TypeArgumentsOpt);
            }
        }
    }
}
