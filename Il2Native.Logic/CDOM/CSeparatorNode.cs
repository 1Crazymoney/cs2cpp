﻿namespace Il2Native.Logic.CDOM
{
    using System.CodeDom.Compiler;

    public class CSeparatorNode : CNode
    {
        public override void WriteTo(IndentedTextWriter itw)
        {
            itw.WriteLine();
        }
    }
}
