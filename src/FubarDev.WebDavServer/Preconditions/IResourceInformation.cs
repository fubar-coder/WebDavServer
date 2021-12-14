// <copyright file="IResourceInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.Preconditions
{
    /// <summary>
    /// Information about a given resource.
    /// </summary>
    public interface IResourceInformation
    {
        /// <summary>
        /// Determines whether the given resource has the given entity tag.
        /// </summary>
        /// <param name="entityTag">The ETag to compare with.</param>
        /// <returns><see langword="true"/> when the entity tag of the given resource matches the given
        /// entity tag.</returns>
        bool HasETag(EntityTag entityTag);

        /// <summary>
        /// Determines whether the given resource has the given state token.
        /// </summary>
        /// <param name="stateToken">The state token to compare with.</param>
        /// <returns><see langword="true"/> when the state token of the given resource matches the given
        /// state token.</returns>
        bool HasStateToken(Uri stateToken);
    }
}
