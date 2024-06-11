
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Xpandables.Net.Compositions;

/// <summary>
/// Extends <see cref="DirectoryCatalog"/> to support discovery 
/// of parts in sub-directories.
/// </summary>
public sealed class RecursiveDirectoryCatalog
    : ComposablePartCatalog, INotifyComposablePartCatalogChanged,
    ICompositionElement
{
    private readonly AggregateCatalog _aggregateCatalog = new();
    private readonly string _path;

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="RecursiveDirectoryCatalog"/> class 
    /// with <see cref="ComposablePartDefinition"/> objects based on all the DLL 
    /// files in the specified directory path and its sub-directories.
    /// </summary>
    /// <param name="path">Path to the directory to scan for assemblies 
    /// to add to the catalog.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="path"/> is null.</exception>
    public RecursiveDirectoryCatalog(string path) : this(path, "*.dll") { }

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="RecursiveDirectoryCatalog"/> class 
    /// with <see cref="ComposablePartDefinition"/> objects based on the 
    /// specified search pattern in the specified directory path and its 
    /// sub-directories.
    /// </summary>
    /// <param name="path">Path to the directory to scan for 
    /// assemblies to add to the catalog.</param>
    /// <param name="searchPattern">The pattern to search with. The format of 
    /// the pattern should be the same as specified for GetFiles.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="path"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="searchPattern"/> is null.</exception>
    public RecursiveDirectoryCatalog(string path, string searchPattern)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _ = searchPattern ?? throw new ArgumentNullException(
            nameof(searchPattern));

        Initialize(path, searchPattern);
    }

    /// <summary>
    /// Gets the part definitions that are contained in the catalog.
    /// </summary>
    /// <returns>The <see cref="ComposablePartDefinition" /> contained in 
    /// the <see cref="ComposablePartCatalog" />.</returns>
    /// <exception cref="ObjectDisposedException">The 
    /// <see cref="ComposablePartCatalog" /> 
    /// object has been disposed of.</exception>
    public override IQueryable<ComposablePartDefinition> Parts
        => _aggregateCatalog.AsQueryable();

    /// <summary>
    /// Gets the display name of the composition element.
    /// </summary>
    /// <returns>The human-readable display name of 
    /// the <see cref="ICompositionElement" />.</returns>
    public string DisplayName => GetDisplayName();

    /// <summary>
    /// Gets the composition element from which the current 
    /// composition element originated.
    /// </summary>
    /// <returns>The composition element from which the 
    /// current <see cref="ICompositionElement" />
    /// originated, or <see langword="null" /> if 
    /// the <see cref="ICompositionElement" /> 
    /// is the root composition element.</returns>
    public ICompositionElement? Origin => null;

    /// <summary>
    /// Occurs when a <see cref="ComposablePartCatalog" /> has changed.
    /// </summary>
    public event EventHandler<ComposablePartCatalogChangeEventArgs>? Changed;

    /// <summary>
    /// Occurs when a <see cref="ComposablePartCatalog" /> is changing.
    /// </summary>
    public event EventHandler<ComposablePartCatalogChangeEventArgs>? Changing;

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
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

    private string GetDisplayName() => $"{GetType().Name} (RecursivePath={_path})";

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
