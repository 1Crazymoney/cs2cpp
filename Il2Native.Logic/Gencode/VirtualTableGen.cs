﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VirtualTableGen.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Il2Native.Logic.Gencode
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using DebugInfo.DebugInfoSymbolWriter;
    using PEAssemblyReader;

    /// <summary>
    /// </summary>
    public static class VirtualTableGen
    {
        /// <summary>
        /// </summary>
        private static readonly IDictionary<string, List<CWriter.Pair<IMethod, IMethod>>> VirtualInterfaceTableByType =
            new SortedDictionary<string, List<CWriter.Pair<IMethod, IMethod>>>();

        /// <summary>
        /// </summary>
        private static readonly IDictionary<string, List<IMethod>> VirtualInterfaceTableLayoutByType =
            new SortedDictionary<string, List<IMethod>>();

        /// <summary>
        /// </summary>
        private static readonly IDictionary<string, List<CWriter.Pair>> VirtualTableByType =
            new SortedDictionary<string, List<CWriter.Pair>>();

        /// <summary>
        /// </summary>
        /// <param name="virtualTable">
        /// </param>
        /// <param name="thisType">
        /// </param>
        /// <param name="interface">
        /// </param>
        public static void BuildVirtualInterfaceTable(
            this List<CWriter.Pair<IMethod, IMethod>> virtualTable,
            IType thisType,
            IType @interface,
            ICodeWriter codeWriter)
        {
            var allExplicit = IlReader.Methods(
                thisType.FindInterfaceOwner(@interface),
                BindingFlags.FlattenHierarchy | BindingFlags.Instance,
                codeWriter)
                .Where(m => !(m is IMethodExtraAttributes && ((IMethodExtraAttributes)m).IsStructObjectAdapter))
                .Where(m => m.IsExplicitInterfaceImplementation).ToList();

            var allPublicAndInternal = IlReader.Methods(
                thisType,
                BindingFlags.FlattenHierarchy | BindingFlags.Instance,
                codeWriter)
                .Where(m => !(m is IMethodExtraAttributes && ((IMethodExtraAttributes)m).IsStructObjectAdapter))
                .Where(m => m.IsPublic || m.IsInternal).Reverse().ToList();

            // we need to use reverse to be able to select first possible method from direved class first
            virtualTable.AddMethodsToVirtualInterfaceTable(thisType, @interface, allExplicit, allPublicAndInternal, codeWriter, thisType.IsArray);
        }

        /// <summary>
        /// </summary>
        /// <param name="virtualTable">
        /// </param>
        /// <param name="thisType">
        /// </param>
        /// <param name="llvmWriter">
        /// </param>
        public static void BuildVirtualTable(
            this List<CWriter.Pair> virtualTable,
            IType thisType,
            ICodeWriter codeWriter)
        {
            if (thisType.BaseType != null)
            {
                virtualTable.BuildVirtualTable(thisType.BaseType, codeWriter);
            }

            // add all interfaces
            virtualTable.AddRange(
                thisType.GetInterfacesExcludingBaseAllInterfaces()
                        .Select(@interface => new CWriter.Pair<IType, IType> { Kind = CWriter.PairKind.Interface, Key = @interface, Value = @interface }));

            // get all virtual methods in current type and replace or append
            foreach (var virtualOrAbstractMethodItem in
                IlReader.Methods(thisType, codeWriter)
                        .Where(m => !(m is IMethodExtraAttributes && ((IMethodExtraAttributes)m).IsStructObjectAdapter))
                        .Where(m => m.IsVirtual || m.IsAbstract || m.IsOverride))
            {
                var virtualOrAbstractMethod = virtualOrAbstractMethodItem;

                Debug.Assert(!virtualOrAbstractMethod.IsGenericMethodDefinition);
                Debug.Assert(!virtualOrAbstractMethod.DeclaringType.IsGenericTypeDefinition);

                if (virtualOrAbstractMethod.IsAbstract && virtualOrAbstractMethod.DeclaringType.Equals(thisType))
                {
                    virtualTable.Add(new CWriter.Pair<IMethod, IMethod> { Key = virtualOrAbstractMethod, Value = virtualOrAbstractMethod });
                    continue;
                }

                // find method in virtual table
                var baseMethod = virtualOrAbstractMethod.IsOverride
                                     ? virtualTable.Where(p => p.Kind == CWriter.PairKind.Method)
                                                   .OfType<CWriter.Pair<IMethod, IMethod>>()
                                                   .LastOrDefault(m => m.Key.IsMatchingOverride(virtualOrAbstractMethod))
                                     : virtualTable.Where(p => p.Kind == CWriter.PairKind.Method)
                                                   .OfType<CWriter.Pair<IMethod, IMethod>>()
                                                   .LastOrDefault(m => m.Key.IsMatchingOverride(virtualOrAbstractMethod));

                Debug.Assert(baseMethod != null || (!virtualOrAbstractMethod.IsOverride && baseMethod == null), "Could not resolve override method");

                if (thisType.IsValueType())
                {
                    // replace virtual/interface method with adapter
                    var adapterMethod =
                        IlReader.Methods(thisType, codeWriter, structObjectAdaptersOnly: true)
                                .FirstOrDefault(
                                    m =>
                                    m is IMethodExtraAttributes && ((IMethodExtraAttributes)m).IsStructObjectAdapter
                                    && ((IMethodExtraAttributes)m).Original.Equals(virtualOrAbstractMethod));

                    ////Debug.Assert(adapterMethod != null, "Could not find adapter method for method: " + virtualOrAbstractMethod.Name);

                    if (adapterMethod != null)
                    {
                        virtualOrAbstractMethod = adapterMethod;
                    }
                }

                if (baseMethod == null)
                {
                    virtualTable.Add(new CWriter.Pair<IMethod, IMethod> { Key = virtualOrAbstractMethod, Value = virtualOrAbstractMethod });
                    continue;
                }

                baseMethod.Value = virtualOrAbstractMethod;
            }
        }

        /// <summary>
        /// </summary>
        public static void Clear()
        {
            VirtualTableByType.Clear();
            VirtualInterfaceTableByType.Clear();
            VirtualInterfaceTableLayoutByType.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="thisType">
        /// </param>
        /// <param name="interface">
        /// </param>
        /// <returns>
        /// </returns>
        public static IEnumerable<CWriter.Pair> GetVirtualInterfaceTable(
            this IType thisType,
            IType @interface,
            ICodeWriter codeWriter)
        {
            List<CWriter.Pair<IMethod, IMethod>> virtualInterfaceTable;

            var key = string.Concat(thisType.FullName, '+', @interface.FullName);
            if (VirtualInterfaceTableByType.TryGetValue(key, out virtualInterfaceTable))
            {
                return virtualInterfaceTable.OfType<CWriter.Pair>();
            }

            virtualInterfaceTable = new List<CWriter.Pair<IMethod, IMethod>>();
            virtualInterfaceTable.BuildVirtualInterfaceTable(thisType, @interface, codeWriter);

            VirtualInterfaceTableByType[key] = virtualInterfaceTable;

            return virtualInterfaceTable;
        }

        /// <summary>
        /// </summary>
        /// <param name="interface">
        /// </param>
        /// <returns>
        /// </returns>
        public static List<IMethod> GetVirtualInterfaceTableLayout(this IType @interface, ICodeWriter codeWriter)
        {
            Debug.Assert(!@interface.SpecialUsage(), "normalize type before using it");

            List<IMethod> virtualInterfaceTableLayout;

            var key = string.Concat(@interface.FullName, '+', @interface.FullName);
            if (VirtualInterfaceTableLayoutByType.TryGetValue(key, out virtualInterfaceTableLayout))
            {
                return virtualInterfaceTableLayout;
            }

            virtualInterfaceTableLayout = new List<IMethod>();
            virtualInterfaceTableLayout.AddMethodsToVirtualInterfaceTableLayout(@interface, codeWriter);

            VirtualInterfaceTableLayoutByType[key] = virtualInterfaceTableLayout;

            return virtualInterfaceTableLayout;
        }

        /// <summary>
        /// </summary>
        /// <param name="type">
        /// </param>
        /// <param name="interface">
        /// </param>
        /// <returns>
        /// </returns>
        public static string GetVirtualInterfaceTableName(this IType type, IType @interface, CWriter cWriter, bool implementation = false)
        {
            Debug.Assert(!type.IsInterface, "Interface is not expected here");
            Debug.Assert(@interface.IsInterface, "Interface expected");
            return
                string.Concat(
                    type.FullName,
                    " vtable ",
                    @interface.FullName,
                    implementation ? " interface_impl" : " interface",
                    type.IsAssemblyNamespaceRequired() ? type.GetAssemblyNamespace(cWriter.AssemblyQualifiedName) : "").CleanUpName();
        }

        [Obsolete("Reduce casting here when interfaces are done")]
        public static string GetVirtualInterfaceTableNameReference(this IType type, IType @interface, CWriter cWriter)
        {
            return string.Concat("(Void **) &", GetVirtualInterfaceTableName(type, @interface, cWriter, true));
        }

        /// <summary>
        /// </summary>
        /// <param name="thisType">
        /// </param>
        /// <param name="codeWriterer">
        /// </param>
        /// <returns>
        /// </returns>
        public static IEnumerable<CWriter.Pair> GetVirtualTable(
            this IType thisType,
            ICodeWriter codeWriter)
        {
            List<CWriter.Pair> virtualTable;

            if (VirtualTableByType.TryGetValue(thisType.FullName, out virtualTable))
            {
                return virtualTable;
            }

            virtualTable = new List<CWriter.Pair>();
            virtualTable.BuildVirtualTable(thisType, codeWriter);

#if DEBUG
            if (!thisType.IsAbstract)
            {
                var issueMethod = virtualTable.Where(v => v.Kind == CWriter.PairKind.Method).OfType<CWriter.Pair<IMethod, IMethod>>().FirstOrDefault(vm => vm.Value == null || vm.Value.IsAbstract);
                Debug.Assert(issueMethod == null, "Not all virtual methods are resolved");
            }
#endif
            VirtualTableByType[thisType.FullName] = virtualTable;

            return virtualTable;
        }

        /// <summary>
        /// </summary>
        /// <param name="type">
        /// </param>
        /// <returns>
        /// </returns>
        public static string GetVirtualTableName(this IType type, CWriter cWriter, bool implementation = false)
        {
            var name = string.Concat(
                type.FullName,
                implementation ? " vtable_impl" : " vtable",
                type.IsAssemblyNamespaceRequired() ? type.GetAssemblyNamespace(cWriter.AssemblyQualifiedName).CleanUpName() : "");

            return name.CleanUpName();
        }

        public static string GetVirtualTableNameReference(this IType type, CWriter cWriter)
        {
            return string.Concat("(Void**) &", GetVirtualTableName(type, cWriter, true));
        }

        /// <summary>
        /// </summary>
        /// <param name="virtualTable">
        /// </param>
        /// <returns>
        /// </returns>
        public static int GetVirtualTableSize(this List<CWriter.Pair<IMethod, IMethod>> virtualTable)
        {
            return virtualTable.Count;
        }

        public static IMethod GetCorrespondingMethodForInterface(this IType thisType, IMethod methodInfo)
        {
            if (methodInfo.DeclaringType.IsInterface)
            {
                var existingMethodInfo = thisType.GetMethods(BindingFlags.Instance).FirstOrDefault(methodInfo.IsMatchingExplicitInterfaceOverride);
                if (existingMethodInfo != null)
                {
                    return existingMethodInfo;
                }
            }

            return thisType.GetMethods(BindingFlags.Instance).FirstOrDefault(m => methodInfo.IsMatchingOverride(m) && thisType.IsAssignableFrom(m.DeclaringType));
        }

        /// <summary>
        /// </summary>
        /// <param name="thisType">
        /// </param>
        /// <param name="methodInfo">
        /// </param>
        /// <returns>
        /// </returns>
        public static bool HasCorrespondingMethodForInterface(this IType thisType, IMethod methodInfo)
        {
            return thisType.GetCorrespondingMethodForInterface(methodInfo) != null;
        }

        /// <summary>
        /// </summary>
        /// <param name="virtualTable">
        /// </param>
        /// <param name="cWriter">
        /// </param>
        /// <param name="type">
        /// </param>
        /// <param name="interfaceIndex">
        /// </param>
        /// <param name="baseTypeFieldsOffset">
        /// </param>
        public static void WriteTableOfMethodsWithImplementation(
            this IEnumerable<CWriter.Pair> virtualTable,
            CWriter cWriter,
            IType type,
            IType interfaceType = null,
            bool declaration = false)
        {
            if (declaration)
            {
                cWriter.Output.Write("extern ");
            }

            WriteVirtualTableImplementationDeclaration(cWriter, type, interfaceType);
            cWriter.Output.Write("[]");
            if (!declaration)
            {
                cWriter.Output.Write(" = ");
                VirtualTableDefinition(virtualTable, interfaceType ?? type, cWriter);
            }

            cWriter.Output.Write(";");
        }

        public static void WriteVirtualTableImplementationDeclaration(
            CWriter cWriter,
            IType type,
            IType interfaceType = null)
        {
            var writer = cWriter.Output;

            writer.Write("const Void* ");
            //VirtualTableDeclaration(virtualTable, interfaceType ?? type, typeResolver);
            //typeResolver.Output.Write(" ");
            if (interfaceType == null)
            {
                writer.Write(type.GetVirtualTableName(cWriter, true));
            }
            else
            {
                writer.Write(type.GetVirtualInterfaceTableName(interfaceType, cWriter, true));
            }
        }

        private static void VirtualTableDefinition(
            IEnumerable<CWriter.Pair> virtualTable, IType type, CWriter cWriter)
        {
            var writer = cWriter.Output;

            writer.WriteLine("{");
            writer.Indent++;

            var first = true;
            // define virtual table
            foreach (var virtualMethodPair in virtualTable)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.WriteLine(",");
                }

                if (virtualMethodPair.Kind == CWriter.PairKind.Interface)
                {
                    var @interfacePair = virtualMethodPair as CWriter.Pair<IType, IType>;
                    var @interface = @interfacePair.Value;
                    var interfaceOwner = type.FindInterfaceOwner(@interface);
                    var requiredInterfaceTableFromCurrentClass = HasVirtualMethodOrExplicitMethod(type, interfaceOwner, @interface, cWriter);

                    writer.Write("(Void*) &");
                    writer.Write((requiredInterfaceTableFromCurrentClass ? type : interfaceOwner).GetVirtualInterfaceTableName(@interface, cWriter, true));

                    continue;
                }

                var virtualMethod = virtualMethodPair as CWriter.Pair<IMethod, IMethod>;

                var method = virtualMethod.Value;

                writer.Write("(");
                //typeResolver.WriteMethodPointerType(writer, methodKey);
                writer.Write("Void*");
                writer.Write(")");
                writer.Write(" ");

                if (method == null || virtualMethod.Value.IsAbstract)
                {
                    writer.Write("&__pure_virtual");
                }
                else
                {
                    // write pointer to method
                    writer.Write("&");
                    cWriter.WriteMethodDefinitionName(writer, method);
                }
            }

            writer.WriteLine(string.Empty);
            writer.Indent--;
            writer.Write("}");
        }

        public static bool HasVirtualMethodOrExplicitMethod(IType type, IType interfaceOwner, IType @interface, ICodeWriter codeWriter)
        {
            Debug.Assert(!type.IsInterface);
            Debug.Assert(!interfaceOwner.IsInterface);
            Debug.Assert(@interface.IsInterface);

            return HasVirtualMethodInInterface(interfaceOwner, @interface, codeWriter) || HasExplicitMethod(type, codeWriter);
        }

        private static bool HasExplicitMethod(IType type, ICodeWriter codeWriter)
        {
            Debug.Assert(!type.IsInterface);
            return IlReader.Methods(type, codeWriter).Any(m => m.IsExplicitInterfaceImplementation);
        }

        private static bool HasVirtualMethodInInterface(IType interfaceOwner, IType @interface, ICodeWriter codeWriter)
        {
            Debug.Assert(!interfaceOwner.IsInterface);
            Debug.Assert(@interface.IsInterface);

            return interfaceOwner.GetVirtualInterfaceTable(@interface, codeWriter)
                .Where(m => m.Kind == CWriter.PairKind.Method)
                .OfType<CWriter.Pair<IMethod, IMethod>>()
                .Any(m => m.Value.IsMethodVirtual() || m.Value.IsExplicitInterfaceImplementation);
        }

        /// <summary>
        /// </summary>
        /// <param name="virtualTable">
        /// </param>
        /// <param name="interface">
        /// </param>
        /// <param name="allPublicAndInternal">
        /// </param>
        private static void AddMethodsToVirtualInterfaceTable(
            this List<CWriter.Pair<IMethod, IMethod>> virtualTable,
            IType type,
            IType @interface,
            IEnumerable<IMethod> allExplicit,
            IEnumerable<IMethod> allPublicAndInternal,
            ICodeWriter codeWriter,
            bool ignoreAssert)
        {
            Debug.Assert(!type.IsInterface, "Interface is not expected");
            Debug.Assert(@interface.IsInterface, "Interface is expected");

            foreach (var baseInterface in @interface.GetInterfaces())
            {
                // get all virtual methods in current type and replace or append
                virtualTable.AddMethodsToVirtualInterfaceTable(type, baseInterface, allExplicit, allPublicAndInternal, codeWriter, ignoreAssert);
            }

            // get all virtual methods in current type and replace or append
#if DEBUG
            var interfaceMethods = IlReader.Methods(@interface, codeWriter).Where(m => !m.IsStatic).ToList();
#else
            var interfaceMethods = IlReader.Methods(@interface, typeResolver).Where(m => !m.IsStatic);
#endif

            ResolveAndAppendInterfaceMethods(virtualTable, type, allExplicit, allPublicAndInternal, interfaceMethods, ignoreAssert, codeWriter);
        }

        private static void ResolveAndAppendInterfaceMethods(List<CWriter.Pair<IMethod, IMethod>> virtualTable, IType type, IEnumerable<IMethod> allExplicit, IEnumerable<IMethod> allPublicAndInternal, IEnumerable<IMethod> interfaceMethods, bool ignoreAssert, ICodeWriter codeWriter)
        {
            Debug.Assert(!type.IsInterface, "Interface is not expected");

            var list =
                interfaceMethods.Select(
                    interfaceMember =>
                    new CWriter.Pair<IMethod, IMethod>
                        {
                            Key = interfaceMember,
                            Value =
                                allExplicit.Where(interfaceMember.IsMatchingInterfaceOverride)
                                         .OrderByDescending(x => x.IsExplicitInterfaceImplementation)
                                         .FirstOrDefault()
                                ?? allPublicAndInternal.Where(interfaceMember.IsMatchingInterfaceOverride)
                                         .OrderByDescending(x => x.IsExplicitInterfaceImplementation)
                                         .FirstOrDefault()
                        }).ToList();

#if DEBUG
            if (!ignoreAssert)
            {
                Debug.Assert(list.All(i => i.Value != null), "Not all method could be resolved");
            }
#endif

            if (type.IsValueType())
            {
                foreach (var interfaceMethod in list)
                {
                    // replace virtual/interface method with adapter
                    var adapterMethod = IlReader.Methods(type, codeWriter, structObjectAdaptersOnly: true)
                        .FirstOrDefault(m => m is IMethodExtraAttributes &&
                                    ((IMethodExtraAttributes)m).Original.Equals(interfaceMethod.Value));

                    ////Debug.Assert(adapterMethod != null, "Could not find adapter method for method: " + virtualOrAbstractMethod.Name);

                    if (adapterMethod != null)
                    {
                        interfaceMethod.Value = adapterMethod;
                    }
                }
            }

            virtualTable.AddRange(list);
        }

        /// <summary>
        /// </summary>
        /// <param name="virtualTable">
        /// </param>
        /// <param name="interface">
        /// </param>
        private static void AddMethodsToVirtualInterfaceTableLayout(this List<IMethod> virtualTable, IType @interface, ICodeWriter codeWriter)
        {
            var baseInterfaces = @interface.GetInterfaces();
            var firstChildInterface = baseInterfaces != null ? baseInterfaces.FirstOrDefault() : null;
            if (firstChildInterface != null)
            {
                // get all virtual methods in current type and replace or append
                virtualTable.AddMethodsToVirtualInterfaceTableLayout(firstChildInterface, codeWriter);
            }

            // get all virtual methods in current type and replace or append
            // if you have internal methods or fields you need to bypass it so for code protection with need to filter all methods by IsAbstract
            virtualTable.AddRange(IlReader.Methods(@interface, codeWriter).Where(m => m.IsAbstract));
        }
    }
}