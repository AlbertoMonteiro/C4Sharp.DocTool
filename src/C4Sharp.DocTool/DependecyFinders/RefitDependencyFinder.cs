using C4Sharp.Models;

namespace C4Sharp.DocTool.DependecyFinders;

internal static class RefitDependencyFinder
{
    internal static bool Find(TypeDeclarationSyntax typeDeclarationSyntax, Compilation compilation, string? xmlDoc)
    {
        if (!typeDeclarationSyntax.IsKind(SyntaxKind.InterfaceDeclaration))
            return false;

        foreach (var method in typeDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
        {
            var symbol = compilation.GetSemanticModel(method.SyntaxTree).GetDeclaredSymbol(method);
            if (symbol?.GetAttributes().Any(x => x.AttributeClass.ContainingAssembly.Name == "Refit") is true)
            {
                var interfaceName = typeDeclarationSyntax.Identifier.ValueText;
                ColorConsole.WriteLine(Timed("Refit dependency found: ".Yellow(), interfaceName.Green()));

                var (owner, containerType, technology, link, boundary, tags, description, relationshipLabel, relationshipProtocol) = xmlDoc.LoadXmlModel();

                var container = new Container(interfaceName, interfaceName)
                {
                    ContainerType = containerType ?? ContainerType.Api,
                    Technology = technology,
                    Link = link,
                    Boundary = boundary,
                    Tags = tags,
                    Description = description,
                };

                ApplicationDiagram.AddContainer(container, owner, relationshipLabel, relationshipProtocol);
                System.Diagnostics.Debug.WriteLine(xmlDoc);
                break;
            }
        }
        return true;
    }
}