// <copyright file="ILockStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// Interface for a lock store.
    /// </summary>
    public interface ILockStore
    {
        /// <summary>
        /// Creates a lock on the given resource.
        /// </summary>
        /// <remarks>
        /// The creation of the lock will fail under the following conditions:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             An existing lock was found that covers this resource with a different share mode.
        ///         </description>
        ///         <description>
        ///             An existing lock was found that covers this resource with an exclusive share mode.
        ///         </description>
        ///         <description>
        ///             The requested lock covers an existing lock with a different share mode.
        ///         </description>
        ///         <description>
        ///             The requested lock has an exclusive share mode and covers an existing lock.
        ///         </description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="lock">The lock to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The created lock.</returns>
        ValueTask<IActiveLock> CreateAsync(
            ILock @lock,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the lock.
        /// </summary>
        /// <param name="stateToken">The ID of the lock.</param>
        /// <param name="owner">The owner of the lock.</param>
        /// <param name="timeout">The new timeout for the lock.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The updated lock.</returns>
        /// <remarks>
        /// When <paramref name="timeout"/> is <see langword="null"/>, then the lock timeout will be restarted
        /// with the initial timeout.
        /// </remarks>
        ValueTask<IActiveLock> RefreshAsync(
            string stateToken,
            XElement? owner,
            TimeSpan? timeout,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases a lock for the given owner.
        /// </summary>
        /// <param name="stateToken">The ID of the lock.</param>
        /// <param name="owner">The owner of the lock.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The task.</returns>
        ValueTask ReleaseAsync(
            string stateToken,
            XElement? owner,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all locks for the given path.
        /// </summary>
        /// <param name="path">The path of the resource to be covered by the returned locks.</param>
        /// <param name="owner">The owner of the locks to be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The found locks.</returns>
        ValueTask<IReadOnlyCollection<IActiveLock>> FindActiveAsync(
            string path,
            XElement? owner,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all locks.
        /// </summary>
        /// <param name="owner">The owner of the locks to be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The found locks.</returns>
        ValueTask<IReadOnlyCollection<IActiveLock>> FindAsync(
            XElement? owner,
            CancellationToken cancellationToken = default);
    }
}
