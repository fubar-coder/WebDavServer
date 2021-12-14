// <copyright file="InMemoryLockStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.Locking.InMemory
{
    /// <summary>
    /// In-memory lock store.
    /// </summary>
    public class InMemoryLockStore : ILockStore
    {
        private readonly MultiValueDictionary<string, InMemoryLock> _locksByPath =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, InMemoryLock> _locksById = new();
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly ISystemClock _systemClock;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryLockStore"/> class.
        /// </summary>
        /// <param name="systemClock">The system clock interface.</param>
        public InMemoryLockStore(ISystemClock systemClock)
        {
            _systemClock = systemClock;
        }

        /// <inheritdoc />
        public ValueTask<IActiveLock> CreateAsync(ILock @lock, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<IActiveLock> RefreshAsync(
            string stateToken,
            XElement? owner,
            TimeSpan? timeout,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_locksById.TryGetValue(stateToken, out var l))
                {
                    throw new KeyNotFoundException($"The lock with the ID {stateToken} was not found.");
                }

                if (owner != null)
                {
                    var lockOwner = l.GetOwner();
                    if (lockOwner == null)
                    {

                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public ValueTask ReleaseAsync(string stateToken, XElement? owner, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<IActiveLock>> FindActiveAsync(string path, XElement? owner, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<IActiveLock>> FindAsync(XElement? owner, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        
        private class InMemoryLock : IActiveLock
        {
            private readonly ILock _lock;

            public InMemoryLock(
                ILock @lock,
                ISystemClock systemClock)
            {
                var now = systemClock.UtcNow;
                _lock = @lock;
                Timeout = @lock.Timeout;
                Issued = now;
                Expiration = now + Timeout;
            }

            private InMemoryLock(
                IActiveLock @lock,
                ISystemClock systemClock,
                TimeSpan timeout)
            {
                var now = systemClock.UtcNow;
                _lock = @lock;
                Timeout = timeout;
                Issued = @lock.Issued;
                Expiration = now + Timeout;
                LastRefresh = now;
            }

            /// <inheritdoc />
            public string Path => _lock.Path;

            /// <inheritdoc />
            public string Href => _lock.Href;

            /// <inheritdoc />
            public bool Recursive => _lock.Recursive;

            /// <inheritdoc />
            public string AccessType => _lock.AccessType;

            /// <inheritdoc />
            public string ShareMode => _lock.ShareMode;

            /// <inheritdoc />
            public TimeSpan Timeout { get; }

            /// <inheritdoc />
            public XElement? GetOwner()
            {
                return _lock.GetOwner();
            }

            /// <inheritdoc />
            public string StateToken { get; } = Guid.NewGuid().ToString("D");

            /// <inheritdoc />
            public DateTime Issued { get; }

            /// <inheritdoc />
            public DateTime? LastRefresh { get; }

            /// <inheritdoc />
            public DateTime Expiration { get; }

            /// <summary>
            /// Performs a refresh on the lock token.
            /// </summary>
            /// <param name="systemClock">The system clock interface.</param>
            /// <param name="timeout">The new timeout.</param>
            /// <returns>The updated lock.</returns>
            [Pure]
            public InMemoryLock Refresh(ISystemClock systemClock, TimeSpan? timeout)
            {
                return new InMemoryLock(this, systemClock, timeout ?? Timeout);
            }
        }
    }
}
