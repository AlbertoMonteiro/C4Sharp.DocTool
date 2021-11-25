using C4Sharp.DocTool;
using C4Sharp.Models.Plantuml.IO;
using System.CommandLine.Parsing;
using System.Diagnostics;

var rootCommand = new RootCommand();

var slnOption = new Option<string>(new[] { "-s", "--slnPath" }, getDefaultValue: () => ".", "The solution file path, if not informed it will search for the first sln file in current directory")
{
    IsRequired = true,
};
slnOption.AddValidator(option =>
{
    var path = option.GetValueOrDefault<string>();

    return path switch
    {
        "." => null,
        _ when !File.Exists(path) => "The specified solution file does not exists",
        _ when Path.GetExtension(path) != ".sln" => "This file is not a solution file",
        _ => null
    };
});
rootCommand.AddOption(slnOption);

rootCommand.Handler = CommandHandler.Create<string>(async slnPath =>
{
    if (slnPath == ".")
    {
        var slnFound = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.sln").FirstOrDefault();

        if (string.IsNullOrWhiteSpace(slnFound))
        {
            ColorConsole.WriteLine(Timed("No solution was found in current directory".Red()));
            return 1;
        }
        slnPath = slnFound;
        ColorConsole.WriteLine(Timed("Solution found: ".White(), Path.GetFileName(slnPath).Green()));
    }

    if (!MSBuildLocator.IsRegistered)
    {
        var instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
        MSBuildLocator.RegisterInstance(instances.OrderByDescending(x => x.Version).First());
    }

    var workspace = MSBuildWorkspace.Create();
    workspace.SkipUnrecognizedProjects = true;

    ColorConsole.WriteLine(Timed("Running dotnet build".White()));
    var dotnetProcess = Process.Start(new ProcessStartInfo(@"dotnet", $"build \"{slnPath}\"")
    {
        RedirectStandardError = true,
        RedirectStandardInput = true,
        RedirectStandardOutput = true
    });
    dotnetProcess!.WaitForExit();
    ColorConsole.WriteLine(Timed("dotnet build complete".White()));

    ColorConsole.WriteLine(Timed("Starting analysis".White()));
    ApplicationDiagram.Init(slnPath);
    var solution = await workspace.OpenSolutionAsync(slnPath);

    foreach (var project in solution.Projects)
    {
        ColorConsole.WriteLine(Timed("Analyzing project: ".White(), project.Name.Green()));
        var compilation = await project.GetCompilationAsync();
        foreach (var document in project.Documents)
        {
            var root = await document.GetSyntaxRootAsync();

            foreach (var typeDeclarationSyntax in root!.DescendantNodes().OfType<TypeDeclarationSyntax>().Where(t => t.IsKind(SyntaxKind.ClassDeclaration) || t.IsKind(SyntaxKind.InterfaceDeclaration)))
            {
                var xmlDoc = compilation!.GetSemanticModel(typeDeclarationSyntax.SyntaxTree).GetDeclaredSymbol(typeDeclarationSyntax)?.GetDocumentationCommentXml();

                if (RefitDependencyFinder.Find(typeDeclarationSyntax, compilation, xmlDoc))
                    continue;
                if (EntityFrameworkCoreDependencyFinder.Find(typeDeclarationSyntax, compilation, xmlDoc))
                    continue;
                if (GenericTypeDependencyFinder.Find(typeDeclarationSyntax, compilation, xmlDoc))
                    continue;
            }
        }
    }
    ColorConsole.WriteLine(Timed("Solution analysis is completed".White()));
    ColorConsole.WriteLine(Timed("Generating C4 diagram".White()));

    var diagram = ApplicationDiagram.BuildContainerDiagram();

    new PlantumlSession()
        .UseDiagramImageBuilder()
        .UseStandardLibraryBaseUrl()
        .Export(Path.Combine(Environment.CurrentDirectory, "c4"), new[] { diagram });

    var image = Directory.EnumerateFiles(Path.Combine(Environment.CurrentDirectory, "c4"), "*.png").First();

    ColorConsole.WriteLine(Timed("C4 diagram generated: ".White(), $"file:///{image.Replace('\\', '/')}".Green()));

    return 0;
});

return await rootCommand.InvokeAsync(args);
