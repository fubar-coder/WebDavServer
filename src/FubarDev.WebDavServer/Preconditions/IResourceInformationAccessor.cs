// <copyright file="IResourceInformationAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Preconditions
{
    /// <summary>
    /// Accessor for the resource information.
    /// </summary>
    public interface IResourceInformationAccessor
    {
        /// <summary>
        /// Gets the resource information for the requested resource.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The resource information for the requested resource.</returns>
        ValueTask<IResourceInformation> GetRequestInformationAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the resource information for the resource pointed to by <paramref name="resourceTag"/>.
        /// </summary>
        /// <param name="reference">The reference to the resource to get the information for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The found resource information.</returns>
        ValueTask<IResourceInformation> GetResourceInformationAsync(
            string reference,
            CancellationToken cancellationToken = default);
    }
}
