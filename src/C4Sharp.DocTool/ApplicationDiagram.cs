using C4Sharp.Diagrams.Core;
using C4Sharp.Models;
using C4Sharp.Models.Relationships;

namespace C4Sharp.DocTool;

internal static class ApplicationDiagram
{
    public static Person App { get; private set; } = new("app", "");

    public static Dictionary<string, SoftwareSystemBoundary> SoftwareSystemBoundaries { get; } = new();

    private static List<Relationship> _relationships = new();
    private static List<Structure> _structures = new();

    public static void Init(string slnPath)
    {
        App = new Person("app", Path.GetFileNameWithoutExtension(slnPath));
        _structures.Add(App);
    }

    public static void AddContainer(Container container, string? owner = null, string? relationShipLabel = null, string? relationShipProtocol = null)
    {
        if (!string.IsNullOrWhiteSpace(owner))
        {
            if (SoftwareSystemBoundaries.ContainsKey(owner) && SoftwareSystemBoundaries[owner].Containers is List<Container> containers)
                containers.Add(container);
            else
            {
                SoftwareSystemBoundary softSystemBoundary = new(owner, owner) { Containers = new List<Container> { container } };
                SoftwareSystemBoundaries.Add(owner, softSystemBoundary);
                _structures.Add(softSystemBoundary);
            }
        }
        else
        {
            _structures.Add(container);
        }
        Relationship relationship = (relationShipLabel, relationShipProtocol) switch
        {
            ({ Length: > 0 } label, null) => (App > container)[label],
            ({ Length: > 0 } label, { Length: > 0 } protocol) => (App > container)[label, protocol],
            _ => App > container,
        };
        _relationships.Add(relationship);
    }

    public static ContainerDiagram BuildContainerDiagram()
    {
        return new ContainerDiagram
        {
            Title = $"Container diagram for {App!.Alias}",
            Structures = _structures.ToArray(),
            Relationships = _relationships.ToArray()
        };
    }
}
