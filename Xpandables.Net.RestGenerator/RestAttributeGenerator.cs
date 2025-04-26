using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using AttributeData = Microsoft.CodeAnalysis.AttributeData;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using GeneratorSyntaxContext = Microsoft.CodeAnalysis.GeneratorSyntaxContext;
using IIncrementalGenerator = Microsoft.CodeAnalysis.IIncrementalGenerator;
using INamedTypeSymbol = Microsoft.CodeAnalysis.INamedTypeSymbol;
using IncrementalGeneratorInitializationContext = Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext;
using Location = Microsoft.CodeAnalysis.Location;
using ModelExtensions = Microsoft.CodeAnalysis.ModelExtensions;
using SemanticModel = Microsoft.CodeAnalysis.SemanticModel;
using SourceProductionContext = Microsoft.CodeAnalysis.SourceProductionContext;
using SyntaxNode = Microsoft.CodeAnalysis.SyntaxNode;

namespace Xpandables.Net.RestGenerator;

/// <summary>
/// </summary>
[Generator]
public sealed class RestAttributeGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the generator for classes decorated with any RestAttribute
        IncrementalValuesProvider<ClassDeclarationInfo?> requestClasses =
            context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (s, _) => IsPotentialRestRequestClass(s),
                    static (ctx, _) => GetRestRequestClassForGeneration(ctx)).Where(static m => m is not null);

        // Generate code for each request class
        context.RegisterSourceOutput(requestClasses,
            static (spc, request) => GenerateRestAttributeBuilder(spc, request!));
    }

    private static bool IsPotentialRestRequestClass(SyntaxNode node)
    {
        // Quick check to see if this node could be a class with attributes
        if (node is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        // Check if it has at least one attribute list
        return classDeclaration.AttributeLists.Count > 0;
    }

    private static ClassDeclarationInfo? GetRestRequestClassForGeneration(GeneratorSyntaxContext context)
    {
        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;

        // Get the class symbol
        if (ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        // Check if the class already implements IRestAttributeBuilder
        bool implementsInterface = classSymbol.AllInterfaces.Any(i =>
            i.ToDisplayString() == "Xpandables.Net.Executions.Rests.IRestAttributeBuilder");

        if (implementsInterface)
        {
            return null;
        }

        // Find RestAttribute derived attribute on the class
        INamedTypeSymbol? restAttribute = FindRestAttributeOnClass(classSymbol);
        if (restAttribute == null)
        {
            return null;
        }

        // Get the namespace info
        BaseNamespaceDeclarationSyntax? namespaceNode =
            classDeclaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        string namespaceName = namespaceNode != null
            ? ModelExtensions.GetDeclaredSymbol(semanticModel, namespaceNode)?.ToDisplayString() ?? string.Empty
            : string.Empty;

        // Check if the class is already marked as partial
        bool isPartial = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

        return new ClassDeclarationInfo
        {
            ClassDeclaration = classDeclaration,
            ClassSymbol = classSymbol,
            RestAttributeType = restAttribute,
            Namespace = namespaceName,
            IsPartial = isPartial
        };
    }

    private static INamedTypeSymbol? FindRestAttributeOnClass(INamedTypeSymbol classSymbol)
    {
        ImmutableArray<AttributeData> attributes = classSymbol.GetAttributes();

        foreach (AttributeData attribute in attributes)
        {
            INamedTypeSymbol? attributeType = attribute.AttributeClass;
            if (attributeType == null)
            {
                continue;
            }

            // Check if it's derived from _RestAttribute
            INamedTypeSymbol? baseType = attributeType.BaseType;
            while (baseType != null)
            {
                if (baseType.ToDisplayString() == "Xpandables.Net.Executions.Rests._RestAttribute")
                {
                    return attributeType;
                }

                baseType = baseType.BaseType;
            }
        }

        return null;
    }

    private static void GenerateRestAttributeBuilder(SourceProductionContext context, ClassDeclarationInfo classInfo)
    {
        // If the class isn't marked as partial, we need to report a diagnostic
        if (!classInfo.IsPartial)
        {
            DiagnosticDescriptor descriptor = new(
                "XPN001",
                "Class should be partial",
                "The class '{0}' should be marked as partial to enable REST attribute generation",
                "RestGenerator",
                DiagnosticSeverity.Error,
                true);

            Location location = classInfo.ClassDeclaration.Identifier.GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, classInfo.ClassSymbol.Name));
            return;
        }

        // Generate the partial class implementation
        string generatedCode = GeneratePartialClass(classInfo);

        // Get the name for the generated file
        string fileName = $"{classInfo.ClassSymbol.Name}.RestAttributeBuilder.cs";
        string folderPath = "XpandablesGeneration/RestAttributes";
        string fullFileName = $"{folderPath}/{fileName}";

        context.AddSource(fullFileName, SourceText.From(generatedCode, Encoding.UTF8));
    }

    private static string GeneratePartialClass(ClassDeclarationInfo classInfo)
    {
        StringBuilder sb = new();

        // Add copyright header
        sb.AppendLine(@"/*******************************************************************************
 * This file is auto-generated by Xpandables.Net.RestGenerator
 * Do not modify this file directly as changes may be lost on the next build.
 *******************************************************************************/");

        // Add usings
        sb.AppendLine("using System;");
        sb.AppendLine("using Xpandables.Net.Executions.Rests;");
        sb.AppendLine();

        // Add namespace
        if (!string.IsNullOrEmpty(classInfo.Namespace))
        {
            sb.AppendLine($"namespace {classInfo.Namespace};");
            sb.AppendLine();
        }

        // Start the partial class
        string className = classInfo.ClassSymbol.Name;
        sb.AppendLine($"partial class {className} : IRestAttributeBuilder");
        sb.AppendLine("{");

        // Add the Build method
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Builds the REST attribute for this request.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"serviceProvider\">The service provider, if needed.</param>");
        sb.AppendLine("    /// <returns>The configured REST attribute for this request.</returns>");
        sb.AppendLine("    public _RestAttribute Build(IServiceProvider serviceProvider)");
        sb.AppendLine("    {");

        // Get the attribute construction information
        sb.AppendLine("        // Return new instance of the attribute from the class decoration");
        sb.AppendLine($"        return new {classInfo.RestAttributeType.Name}();");
        sb.AppendLine("    }");

        // Close the class
        sb.AppendLine("}");

        return sb.ToString();
    }
}

/// <summary>
/// Information about a REST request class that needs code generation.
/// </summary>
internal sealed class ClassDeclarationInfo
{
    public ClassDeclarationSyntax ClassDeclaration { get; set; } = null!;
    public INamedTypeSymbol ClassSymbol { get; set; } = null!;
    public INamedTypeSymbol RestAttributeType { get; set; } = null!;
    public string Namespace { get; set; } = null!;
    public bool IsPartial { get; set; }
}