﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Symbols;

    public class SwitchSection : Block
    {
        private readonly IList<SwitchLabel> labels = new List<SwitchLabel>();

        public override Kinds Kind
        {
            get { return Kinds.SwitchSection; }
        }

        public IList<SwitchLabel> Labels
        {
            get
            {
                return this.labels;
            }
        }

        public ITypeSymbol SwitchType { get; set; }

        internal void Parse(BoundSwitchSection boundSwitchSection)
        {
            base.Parse(boundSwitchSection);
            foreach (var boundSwitchLabel in boundSwitchSection.SwitchLabels)
            {
                var sourceLabelSymbol = (boundSwitchLabel.Label as SourceLabelSymbol);
                if (sourceLabelSymbol != null)
                {
                    var switchLabel = new SwitchLabel();
                    switchLabel.Parse(sourceLabelSymbol);
                    this.Labels.Add(switchLabel);
                }
            }
        }

        internal override void Visit(Action<Base> visitor)
        {
            base.Visit(visitor);
            foreach (var switchLabel in this.labels)
            {
                switchLabel.Visit(visitor);
            }
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            c.DecrementIndent();

            foreach (var label in this.Labels)
            {
                if (label.Value != null)
                {
                    var skipCaseForNullableWhenNull = this.SwitchType.IsValueType && label.Value.IsNull;
                    if (!skipCaseForNullableWhenNull)
                    {
                        c.TextSpan("case");
                        c.WhiteSpace();

                        if (this.SwitchType.TypeKind == TypeKind.Enum)
                        {
                            c.TextSpan("(");
                            c.WriteType(this.SwitchType);
                            c.TextSpan(")");
                        }

                        c.TextSpan(label.ToString());
                        c.TextSpan(":");
                    }
                }
                else
                {
                    c.TextSpan("default:");
                }

                c.NewLine();

                if (label.GenerateLabel)
                {
                    label.WriteTo(c);
                    c.TextSpan(":");
                    c.NewLine();
                }
            }

            c.IncrementIndent();

            NoParenthesis = true;
            base.WriteTo(c);
        }
    }
}
