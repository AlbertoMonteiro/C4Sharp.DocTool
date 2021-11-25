using C4Sharp.Models;

namespace C4Sharp.DocTool.DependecyFinders;

internal static class GenericTypeDependencyFinder
{
    internal static bool Find(TypeDeclarationSyntax typeDeclarationSyntax, Compilation compilation, string? xmlDoc)
    {
        if (string.IsNullOrEmpty(xmlDoc))
            return false;

        var symbol = compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree).GetDeclaredSymbol(typeDeclarationSyntax);

        var interfaceName = typeDeclarationSyntax.Identifier.ValueText;
        ColorConsole.WriteLine(Timed("Generic dependency found: ".Yellow(), interfaceName.Green()));

        var (owner, containerType, technology, link, boundary, tags, description, relationshipLabel, relationshipProtocol) = xmlDoc.LoadXmlModel();

        var container = new Container(interfaceName, interfaceName)
        {
            ContainerType = containerType ?? ContainerType.None,
            Technology = technology,
            Link = link,
            Boundary = boundary,
            Tags = tags,
            Description = description,
        };

        ApplicationDiagram.AddContainer(container, owner, relationshipLabel, relationshipProtocol);
        return true;
    }
}
