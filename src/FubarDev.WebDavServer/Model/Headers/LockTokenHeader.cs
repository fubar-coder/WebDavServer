// <copyright file="LockTokenHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// The <c>Lock-Token</c> header.
    /// </summary>
    public class LockTokenHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockTokenHeader"/> class.
        /// </summary>
        /// <param name="stateToken">The lock token.</param>
        public LockTokenHeader(Uri stateToken)
        {
            StateToken = stateToken;
        }

        /// <summary>
        /// Gets the lock token.
        /// </summary>
        public Uri StateToken { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"<{StateToken}>";
        }
    }
}
