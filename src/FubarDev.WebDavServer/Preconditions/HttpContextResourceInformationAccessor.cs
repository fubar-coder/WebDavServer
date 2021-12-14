// <copyright file="HttpContextResourceInformationAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Preconditions
{
    /// <summary>
    /// Default implementation of <see cref="IResourceInformationAccessor"/>.
    /// </summary>
    public class HttpContextResourceInformationAccessor
        : IResourceInformationAccessor
    {
        private readonly IWebDavContextAccessor _contextAccessor;
        private readonly IUriComparer _uriComparer;
        private readonly ILogger<HttpContextResourceInformationAccessor>? _logger;
        private readonly Lazy<ILockManager?> _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextResourceInformationAccessor"/> class.
        /// </summary>
        /// <param name="contextAccessor">Accessor for the WebDAV context.</param>
        /// <param name="uriComparer">Comparer for URLs.</param>
        /// <param name="logger">The logger.</param>
        public HttpContextResourceInformationAccessor(
            IWebDavContextAccessor contextAccessor,
            IUriComparer uriComparer,
            ILogger<HttpContextResourceInformationAccessor>? logger = null)
        {
            _contextAccessor = contextAccessor;
            _uriComparer = uriComparer;
            _logger = logger;
            _lockManager = new Lazy<ILockManager?>(
                () => contextAccessor.WebDavContext.RequestServices.GetService<ILockManager>());
        }

        /// <summary>
        /// Gets the lock manager.
        /// </summary>
        private ILockManager? LockManager => _lockManager.Value;

        /// <inheritdoc />
        public async ValueTask<IResourceInformation> GetRequestInformationAsync(CancellationToken cancellationToken = default)
        {
            var context = _contextAccessor.WebDavContext;
            var fileSystem = context.RequestServices.GetRequiredService<IFileSystem>();
            var path = context.ActionUrl.OriginalString;
            var result = await fileSystem.SelectAsync(path, cancellationToken)
                .ConfigureAwait(false);
            var activeLocks = await GetActiveLocksAsync(path, cancellationToken).ConfigureAwait(false);
            if (!result.IsMissing)
            {
                var etag = await result.TargetEntry.GetEntityTagAsync(cancellationToken)
                    .ConfigureAwait(false);
                return new ResourceInfo(etag, activeLocks);
            }

            return new ResourceInfo(null, activeLocks);
        }

        /// <inheritdoc />
        public async ValueTask<IResourceInformation> GetResourceInformationAsync(string reference, CancellationToken cancellationToken = default)
        {
            var referenceUri = new Uri(reference);
            if (!_uriComparer.IsThisServer(referenceUri))
            {
                _logger.LogWarning(
                    "The Resource-Tag {ResourceTag} points to a different server or service",
                    reference);
                return new ResourceInfo(null, ImmutableArray<IActiveLock>.Empty);
            }

            var context = _contextAccessor.WebDavContext;
            var fileSystem = context.RequestServices.GetRequiredService<IFileSystem>();
            var path = context.PublicControllerUrl.MakeRelativeUri(referenceUri).ToString();
            var result = await fileSystem.SelectAsync(path, cancellationToken)
                .ConfigureAwait(false);
            var activeLocks = await GetActiveLocksAsync(path, cancellationToken).ConfigureAwait(false);
            if (!result.IsMissing)
            {
                var etag = await result.TargetEntry.GetEntityTagAsync(cancellationToken)
                    .ConfigureAwait(false);
                return new ResourceInfo(etag, activeLocks);
            }

            return new ResourceInfo(null, activeLocks);
        }

        private async ValueTask<IReadOnlyCollection<IActiveLock>> GetActiveLocksAsync(
            string path,
            CancellationToken cancellationToken)
        {
            var lockManager = LockManager;
            if (lockManager == null)
            {
                return Array.Empty<IActiveLock>();
            }

            return (await lockManager.GetAffectedLocksAsync(path, false, true, cancellationToken)
                    .ConfigureAwait(false))
                .ToImmutableList();
        }

        private class ResourceInfo : IResourceInformation
        {
            private readonly EntityTag? _entityTag;
            private readonly IReadOnlyCollection<IActiveLock> _activeLocks;

            public ResourceInfo(
                EntityTag? entityTag,
                IReadOnlyCollection<IActiveLock> activeLocks)
            {
                _entityTag = entityTag;
                _activeLocks = activeLocks;
            }

            /// <inheritdoc />
            public bool HasETag(EntityTag entityTag)
            {
                if (!_entityTag.HasValue)
                {
                    return false;
                }

                return EntityTagComparer.Weak.Equals(entityTag, _entityTag.Value);
            }

            /// <inheritdoc />
            public bool HasStateToken(Uri stateToken)
            {
                var stateTokenText = stateToken.OriginalString;
                return _activeLocks.Any(l => l.StateToken == stateTokenText);
            }
        }
    }
}
