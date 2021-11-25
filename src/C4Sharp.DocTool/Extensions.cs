using C4Sharp.Models;
using C4Sharp.Models.Relationships;
using System.Xml.Linq;

namespace C4Sharp.DocTool;

public static class Extensions
{
    public static ColorToken[] Timed(params ColorToken[] texts)
    {
        Span<ColorToken> a = new ColorToken[3 + texts.Length];
        a[0] = "[".Blue();
        a[1] = DateTime.Now.ToString("T").Blue();
        a[2] = "] ".Blue();
        texts.AsSpan().CopyTo(a[3..]);
        return a.ToArray();
    }

    public static ContainerInfo LoadXmlModel(this string? xmlDoc)
    {
        string? owner = null;
        var containerType = default(ContainerType?);
        var technology = "undefined";
        var link = "";
        var boundary = Boundary.Internal;
        var tags = Array.Empty<string>();
        var description = "";
        var relationshipLabel = default(string);
        var relationshipProtocol = default(string);

        if (!string.IsNullOrWhiteSpace(xmlDoc))
        {
            var xmlRoot = XDocument.Parse(xmlDoc).Root!;
            owner = xmlRoot.Element("C4Owner")?.Value;

            if (Enum.TryParse<ContainerType>(xmlRoot.Element("C4ContainerType")?.Value, out var containerTypeFound))
                containerType = containerTypeFound;

            _ = Enum.TryParse(xmlRoot.Element("C4Boundary")?.Value, out boundary);

            if (xmlRoot.Element("summary")?.Value is string c4Desc && !string.IsNullOrWhiteSpace(c4Desc))
                description = c4Desc.Trim();

            if (xmlRoot.Element("C4Technology")?.Value is string c4Tech && !string.IsNullOrWhiteSpace(c4Tech))
                technology = c4Tech;

            if (xmlRoot.Element("C4Link")?.Value is string c4Link && !string.IsNullOrWhiteSpace(c4Link))
                link = c4Link;

            if (xmlRoot.Element("C4Tags")?.Value is string c4Tags && !string.IsNullOrWhiteSpace(c4Tags))
                tags = c4Tags.Split(',');

            if (xmlRoot.Element("C4RelationshipLabel")?.Value is string c4RelationshipLabel && !string.IsNullOrWhiteSpace(c4RelationshipLabel))
                relationshipLabel = c4RelationshipLabel;

            if (xmlRoot.Element("C4RelationshipProtocol")?.Value is string c4RelationshipProtocol && !string.IsNullOrWhiteSpace(c4RelationshipProtocol))
                relationshipProtocol = c4RelationshipProtocol;
        }
        return new ContainerInfo(owner, containerType, technology, link, boundary, tags, description, relationshipLabel, relationshipProtocol);
    }
}