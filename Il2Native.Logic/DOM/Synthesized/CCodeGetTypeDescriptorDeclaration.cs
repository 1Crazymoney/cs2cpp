﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM.Synthesized
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using DOM2;
    using Implementations;
    using Microsoft.CodeAnalysis;

    public class CCodeGetTypeDescriptorDeclaration : CCodeInternalImplementationMethodDeclaration
    {
        public CCodeGetTypeDescriptorDeclaration(INamedTypeSymbol type)
            : base(type, new GetTypeDescriptorMethod(type))
        {
            this.Type = type;
        }

        protected ITypeSymbol Type { get; set; }

        public override void WriteTo(CCodeWriterBase c)
        {
            c.TextSpanNewLine("inline static GC_descr __get_type_descriptor()");
            c.OpenBlock();
            c.TextSpan("typedef");
            c.WhiteSpace();
            c.WriteType(this.Type, true, true, true);
            c.WhiteSpace();
            c.TextSpanNewLine("__type;");
            c.TextSpanNewLine("GC_word bitmap[GC_BITMAP_SIZE(__type)] = {0};");
            
            // set fields
            foreach (var field in this.Type.EnumPossibleReferenceFields())
            {
                c.TextSpan("GC_set_bit(bitmap, GC_WORD_OFFSET(__type,");
                c.WhiteSpace();
                // TODO: fix it, replace with "base" type as generic types causing issues
                ////c.WriteFieldAccessAsStaticField(field);
                c.WriteName(field);
                c.TextSpanNewLine("));");
            }

            c.TextSpanNewLine("return GC_make_descriptor(bitmap, GC_WORD_LEN(__type));");

            c.EndBlock();
            c.Separate();
        }

        public class GetTypeDescriptorMethod : MethodImpl
        {
            public GetTypeDescriptorMethod(INamedTypeSymbol type)
            {
                Name = "__get_type_descriptor";
                MethodKind = MethodKind.Ordinary;
                ReceiverType = type;
                ContainingType = type;
                Parameters = ImmutableArray<IParameterSymbol>.Empty;
                IsStatic = true;
            }
        }
    }
}
