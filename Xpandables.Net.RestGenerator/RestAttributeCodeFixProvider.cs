using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Xpandables.Net.RestGenerator;

/// <summary>
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RestAttributeCodeFixProvider))]
[Shared]
public sealed class RestAttributeCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("XPN001");

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the class declaration identified by the diagnostic
        ClassDeclarationSyntax? declaration = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .First();

        if (declaration == null)
        {
            return;
        }

        // Register a code action that will invoke the fix
        context.RegisterCodeFix(
            CodeAction.Create(
                "Make class partial",
                c => MakeClassPartialAsync(context.Document, declaration, c),
                "MakeClassPartial"),
            diagnostic);
    }

    private static async Task<Document> MakeClassPartialAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        CancellationToken cancellationToken)
    {
        // Add the partial modifier
        SyntaxTokenList newModifiers = classDecl.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        ClassDeclarationSyntax newClass = classDecl.WithModifiers(newModifiers);

        // Replace the original class with the new one
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SyntaxNode? newRoot = root?.ReplaceNode(classDecl, newClass);

        return document.WithSyntaxRoot(newRoot!);
    }
}