﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM.Implementations
{
    using System;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    class TypeParameterSymbolImpl : ITypeParameterSymbol
    {
        public SymbolKind Kind { get; private set; }

        public string Language { get; private set; }

        public string Name { get; set; }

        public string MetadataName { get; private set; }

        public ISymbol ContainingSymbol { get; private set; }

        public IAssemblySymbol ContainingAssembly { get; private set; }

        public IModuleSymbol ContainingModule { get; private set; }

        public INamedTypeSymbol ContainingType { get; private set; }

        public INamespaceSymbol ContainingNamespace { get; private set; }

        public bool IsDefinition { get; private set; }

        public bool IsStatic { get; private set; }

        public bool IsVirtual { get; private set; }

        public bool IsOverride { get; private set; }

        public bool IsAbstract { get; private set; }

        public bool IsSealed { get; private set; }

        public bool IsExtern { get; private set; }

        public bool IsImplicitlyDeclared { get; private set; }

        public bool CanBeReferencedByName { get; private set; }

        public ImmutableArray<Location> Locations { get; private set; }

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; private set; }

        public ImmutableArray<AttributeData> GetAttributes()
        {
            throw new NotImplementedException();
        }

        public Accessibility DeclaredAccessibility { get; private set; }

        public ITypeParameterSymbol OriginalDefinition { get; private set; }

        public ITypeParameterSymbol ReducedFrom { get; private set; }

        public int Ordinal { get; private set; }

        public VarianceKind Variance { get; private set; }

        public TypeParameterKind TypeParameterKind { get; private set; }

        public IMethodSymbol DeclaringMethod { get; private set; }

        public INamedTypeSymbol DeclaringType { get; private set; }

        public bool HasReferenceTypeConstraint { get; private set; }

        public bool HasValueTypeConstraint { get; private set; }

        public bool HasConstructorConstraint { get; private set; }

        public ImmutableArray<ITypeSymbol> ConstraintTypes { get; private set; }

        ITypeSymbol ITypeSymbol.OriginalDefinition
        {
            get
            {
                return this.OriginalDefinition;
            }
        }

        public SpecialType SpecialType { get; private set; }

        public ISymbol FindImplementationForInterfaceMember(ISymbol interfaceMember)
        {
            throw new NotImplementedException();
        }

        public TypeKind TypeKind { get; set; }

        public INamedTypeSymbol BaseType { get; private set; }

        public ImmutableArray<INamedTypeSymbol> Interfaces { get; private set; }

        public ImmutableArray<INamedTypeSymbol> AllInterfaces { get; private set; }

        public bool IsReferenceType { get; private set; }

        public bool IsValueType { get; private set; }

        public bool IsAnonymousType { get; private set; }

        ISymbol ISymbol.OriginalDefinition
        {
            get
            {
                return this.OriginalDefinition;
            }
        }

        public void Accept(SymbolVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }

        public string GetDocumentationCommentId()
        {
            throw new NotImplementedException();
        }

        public string GetDocumentationCommentXml(
            CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public string ToDisplayString(SymbolDisplayFormat format = null)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null)
        {
            throw new NotImplementedException();
        }

        public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            throw new NotImplementedException();
        }

        public bool HasUnsupportedMetadata { get; private set; }

        public ImmutableArray<ISymbol> GetMembers()
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<ISymbol> GetMembers(string name)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers()
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name, int arity)
        {
            throw new NotImplementedException();
        }

        public bool Equals(ISymbol other)
        {
            throw new NotImplementedException();
        }

        public bool IsNamespace { get; private set; }

        public bool IsType { get; private set; }

        public bool IsTupleType { get; private set; }
    }
}
