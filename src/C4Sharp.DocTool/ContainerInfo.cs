using C4Sharp.Models;
using C4Sharp.Models.Relationships;

namespace C4Sharp.DocTool;

public record struct ContainerInfo(string? Owner,
                                   ContainerType? ContainerType,
                                   string Technology,
                                   string Link,
                                   Boundary Boundary,
                                   string[] Tags,
                                   string Description,
                                   string? RelationshipLabel,
                                   string? RelationshipProtocol);
