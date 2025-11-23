/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Collections;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace System.Primitives.Composition;

/// <summary>
/// Represents an empty catalog of composable parts.
/// </summary>
public sealed class EmptyCatalog : ComposablePartCatalog
{
    /// <inheritdoc/>
    public override IQueryable<ComposablePartDefinition> Parts =>
       new QueryableEmpty<ComposablePartDefinition>();
}

/// <summary>
/// Represents a catalog that recursively searches directories for composable 
/// parts.
/// </summary>
public sealed class RecursiveDirectoryCatalog :
    ComposablePartCatalog,
    INotifyComposablePartCatalogChanged,
    ICompositionElement
{
    private readonly AggregateCatalog _aggregateCatalog = new();
    private readonly string _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveDirectoryCatalog"/> 
    /// class with the specified path.
    /// </summary>
    /// <param name="path">The path to the directory to search.</param>
    public RecursiveDirectoryCatalog(string path) : this(path, "*.dll") { }

    /// <summary>  
    /// Initializes a new instance of the <see cref="RecursiveDirectoryCatalog"/> 
    /// class with the specified path and search pattern.  
    /// </summary>  
    /// <param name="path">The path to the directory to search.</param>  
    /// <param name="searchPattern">The search pattern to match against 
    /// the names of files in the path.</param>  
    /// <exception cref="ArgumentNullException">Thrown when the path or 
    /// search pattern is null.</exception>  
    public RecursiveDirectoryCatalog(string path, string searchPattern)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _ = searchPattern ?? throw new ArgumentNullException(
            nameof(searchPattern));

        Initialize(path, searchPattern);
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public override IQueryable<ComposablePartDefinition> Parts =>
        _aggregateCatalog.AsQueryable();

    /// <inheritdoc/>
    public string DisplayName => GetDisplayName();

    /// <inheritdoc/>
    public ICompositionElement? Origin => null;

    /// <inheritdoc/>
    public event EventHandler<ComposablePartCatalogChangeEventArgs>? Changed;

    /// <inheritdoc/>
    public event EventHandler<ComposablePartCatalogChangeEventArgs>? Changing;

    /// <inheritdoc/>
    public override string ToString() => GetDisplayName();

    private void Initialize(string path, string searchPattern)
    {
        IEnumerable<DirectoryCatalog> directoryCatalogs =
            GetFoldersRecursive(path)
            .Select(dir => new DirectoryCatalog(dir, searchPattern));

        _aggregateCatalog.Changed += (sender, e) => Changed?.Invoke(sender, e);
        _aggregateCatalog.Changing += (sender, e) => Changing?.Invoke(sender, e);

        foreach (DirectoryCatalog? catalog in directoryCatalogs)
        {
            _aggregateCatalog.Catalogs.Add(catalog);
        }
    }

    private string GetDisplayName() =>
        $"{GetType().Name} (RecursivePath={_path})";

    private static List<string> GetFoldersRecursive(string path)
    {
        List<string> result = [path];
        foreach (string child in Directory.GetDirectories(path))
        {
            result.AddRange(GetFoldersRecursive(child));
        }

        return result;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _aggregateCatalog?.Dispose();
        }

        base.Dispose(disposing);
    }
}
