using System.Collections.Generic;
using Etg.SimpleStubs.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen
{
    using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class PropertyStubber : IMethodStubber
    {
        private readonly HashSet<IPropertySymbol> _visited = new HashSet<IPropertySymbol>();

        public ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol,
            INamedTypeSymbol stubbedInterface)
        {
            if (!methodSymbol.IsPropertyAccessor())
            {
                return classDclr;
            }

            IPropertySymbol propertySymbol = (IPropertySymbol)methodSymbol.AssociatedSymbol;
            if (_visited.Contains(propertySymbol))
            {
                return classDclr;
            }
            _visited.Add(propertySymbol);

            string indexerType = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            BasePropertyDeclarationSyntax propDclr = CreatePropertyDclr(methodSymbol, indexerType);

            if (propertySymbol.GetMethod != null)
            {
                IMethodSymbol getMethodSymbol = propertySymbol.GetMethod;
                string parameters = StubbingUtils.FormatParameters(getMethodSymbol);

                string delegateTypeName = NamingUtils.GetDelegateTypeName(getMethodSymbol, stubbedInterface);
                var accessorDclr = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, SyntaxFactory.Block(
                    SyntaxFactory.List(new[]
                    {
                        SyntaxFactory.ParseStatement("return " + StubbingUtils.GenerateInvokeDelegateStmt(delegateTypeName, getMethodSymbol.Name, parameters))
                    })));
                propDclr = propDclr.AddAccessorListAccessors(accessorDclr);
            }
            if (propertySymbol.SetMethod != null)
            {
                IMethodSymbol setMethodSymbol = propertySymbol.SetMethod;
                string parameters = $"{StubbingUtils.FormatParameters(setMethodSymbol)}";
                string delegateTypeName = NamingUtils.GetDelegateTypeName(setMethodSymbol, stubbedInterface);
                var accessorDclr = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, SyntaxFactory.Block(
                    SyntaxFactory.List(new[]
                    {
                        SyntaxFactory.ParseStatement(StubbingUtils.GenerateInvokeDelegateStmt(delegateTypeName, setMethodSymbol.Name, parameters))
                    })));
                propDclr = propDclr.AddAccessorListAccessors(accessorDclr);
            }

            classDclr = classDclr.AddMembers(propDclr);

            return classDclr;
        }

        private BasePropertyDeclarationSyntax CreatePropertyDclr(IMethodSymbol methodSymbol, string propType)
        {
            if (methodSymbol.IsIndexerAccessor())
            {
                IndexerDeclarationSyntax indexerDclr = SyntaxFactory.IndexerDeclaration(
                    SyntaxFactory.ParseTypeName(propType))
                    .WithExplicitInterfaceSpecifier(SyntaxFactory.ExplicitInterfaceSpecifier(
                        SyntaxFactory.IdentifierName(methodSymbol.GetContainingInterfaceGenericQualifiedName())));
                indexerDclr =
                    indexerDclr.AddParameterListParameters(
                        RoslynUtils.GetMethodParameterSyntaxList(methodSymbol).ToArray());
                return indexerDclr;
            }

            string propName = methodSymbol.AssociatedSymbol.Name;
            PropertyDeclarationSyntax propDclr = SF.PropertyDeclaration(SF.ParseTypeName(propType), SF.Identifier(propName))
            .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(
                SF.IdentifierName(methodSymbol.GetContainingInterfaceGenericQualifiedName())));
            return propDclr;
        }
    }
}