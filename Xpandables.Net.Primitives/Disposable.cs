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
namespace Xpandables.Net;

/// <summary>
/// The default implementation for <see cref="IDisposable"/> interface.
/// Every derived class should override the <see cref="Dispose(bool)"/>
/// to match its requirement.
/// This is an <see langword="abstract"/> and serializable class.
/// </summary>
[Serializable]
public abstract class Disposable : IDisposable
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    ///  <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>Default initialization for 
    /// a <see cref="bool"/> is <c>false</c>.</remarks>
    protected bool IsDisposed { get; set; }

    /// <summary>
    /// Public Implementation of Dispose according to .NET Framework 
    /// Design Guidelines
    /// callable by consumers.
    /// Do not make this method virtual.
    /// A derived class should not be able to override this method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This object will be cleaned up by the Dispose method.
    /// Therefore, you should call GC.SuppressFinalize to take 
    /// this object off the finalization queue
    /// and prevent finalization code for this object 
    /// from executing a second time.
    /// </para>
    /// <para>Always use SuppressFinalize() in case 
    /// a subclass of this type implements a finalizer.</para>
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// When overridden in derived classes, this method get 
    /// called when the instance will be disposed.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to 
    /// release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// <list type="bulleted">
    /// <see cref="Dispose(bool)"/> executes in two distinct scenarios.
    /// <item>If <paramref name="disposing"/> equals <c>true</c>, 
    /// the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged 
    /// resources can be disposed.</item>
    /// <item>If <paramref name="disposing"/> equals <c>false</c>, 
    /// the method has been called
    /// by the runtime from inside the finalizer and you should 
    /// not reference other objects.
    /// Only unmanaged resources can be disposed.</item></list>
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            // Release all managed resources here
            // Need to unregister/detach yourself from the events.
            // Always make sure the object is not null first before trying to
            // unregister/detach them!
            // Failure to unregister can be a BIG source of memory leaks
        }

        // Release all unmanaged resources here and override a finalizer below.
        // Set large fields to null.

        // Dispose has been called.
        IsDisposed = true;

        // If it is available, make the call to the
        // base class's Dispose(boolean) method
    }
}