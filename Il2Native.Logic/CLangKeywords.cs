﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic
{
    using System.Collections.Generic;

    public static class CLangKeywords
    {
        private static readonly IDictionary<string, bool> keywords;

        static CLangKeywords()
        {
            var keys = new[]
            {
                "alignas",
                "alignof",
                "and",
                "and_eq",
                "asm",
                "_asm",
                "__asm",
                "auto",
                "bitand",
                "bitor",
                "bool",
                "break",
                "case",
                "catch",
                "char",
                "char16_t",
                "char32_t",
                "class",
                "compl",
                "concept",
                "const",
                "constexpr",
                "const_cast",
                "continue",
                "decltype",
                "default",
                "delete",
                "do",
                "double",
                "dynamic_cast",
                "else",
                "enum",
                "explicit",
                "export",
                "extern",
                "false",
                "float",
                "for",
                "friend",
                "goto",
                "if",
                "inline",
                "int",
                "long",
                "mutable",
                "namespace",
                "new",
                "noexcept",
                "not",
                "not_eq",
                "nullptr",
                "operator",
                "or",
                "or_eq",
                "object",
                "private",
                "protected",
                "public",
                "register",
                "reinterpret_cast",
                "requires",
                "return",
                "short",
                "signed",
                "sizeof",
                "static",
                "static_assert",
                "static_cast",
                "stdin",
                "stdout",
                "stderr",
                "struct",
                "string",
                "switch",
                "template",
                "this",
                "thread_local",
                "throw",
                "true",
                "try",
                "typedef",
                "typeid",
                "typename",
                "union",
                "unsigned",
                "using",
                "virtual",
                "void",
                "volatile",
                "wchar_t",
                "while",
                "xor",
                "xor_eq"
            };

            keywords = new SortedDictionary<string, bool>();
            foreach (var key in keys)
            {
                keywords[key] = false;
            }
        }

        public static string EnsureCompatible(this string value)
        {
            bool val;
            if (keywords.TryGetValue(value, out val))
            {
                return "_" + value;
            }

            return value;
        }

        public static bool IsKeyword(string keyword)
        {
            bool val;
            return keywords.TryGetValue(keyword, out val);
        }
    }
}
