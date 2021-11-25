using C4Sharp.Models;

namespace C4Sharp.DocTool.DependecyFinders;

internal static class EntityFrameworkCoreDependencyFinder
{
    internal static bool Find(TypeDeclarationSyntax typeDeclarationSyntax, Compilation compilation, string? xmlDoc)
    {
        if (!typeDeclarationSyntax.IsKind(SyntaxKind.ClassDeclaration))
            return false;

        var symbol = compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree).GetDeclaredSymbol(typeDeclarationSyntax);

        if (symbol?.BaseType?.ContainingAssembly.Name == "Microsoft.EntityFrameworkCore")
        {
            var interfaceName = typeDeclarationSyntax.Identifier.ValueText;
            ColorConsole.WriteLine(Timed("EntityFrameworkCore dependency found: ".Yellow(), interfaceName.Green()));

            var (owner, containerType, technology, link, boundary, tags, description, relationshipLabel, relationshipProtocol) = xmlDoc.LoadXmlModel();

            var container = new Container(interfaceName, interfaceName)
            {
                ContainerType = containerType ?? ContainerType.Database,
                Technology = technology,
                Link = link,
                Boundary = boundary,
                Tags = tags,
                Description = description,
            };

            ApplicationDiagram.AddContainer(container, owner, relationshipLabel, relationshipProtocol);
            System.Diagnostics.Debug.WriteLine(xmlDoc);
        }
        return true;
    }
}
