// <copyright file="IfHeaderCondition.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Represents a single condition for an HTTP <c>If</c> header.
    /// </summary>
    public readonly struct IfHeaderCondition
    {
        private readonly bool _not;
        private readonly Uri? _stateToken;
        private readonly EntityTag? _etag;

        /// <summary>
        /// Initializes a new instance of the <see cref="IfHeaderCondition"/> struct.
        /// </summary>
        /// <param name="not">Indicates that the condition should succeed if the state token or ETag don't
        /// match.</param>
        /// <param name="stateToken">The state token.</param>
        internal IfHeaderCondition(bool not, Uri stateToken)
        {
            _not = not;
            _etag = null;
            _stateToken = stateToken;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IfHeaderCondition"/> struct.
        /// </summary>
        /// <param name="not">Indicates that the condition should succeed if the state token or ETag don't
        /// match.</param>
        /// <param name="etag">The entity tag.</param>
        internal IfHeaderCondition(bool not, EntityTag etag)
        {
            _not = not;
            _etag = etag;
            _stateToken = null;
        }

        /// <summary>
        /// Gets a value indicating whether the result should be negated.
        /// </summary>
        public bool Not => _not;

        /// <summary>
        /// Gets the state token to validate with.
        /// </summary>
        public Uri? StateToken => _stateToken;

        /// <summary>
        /// Gets the entity tag to validate with.
        /// </summary>
        public EntityTag? ETag => _etag;

        /// <summary>
        /// Gets a value indicating whether this condition is valid.
        /// </summary>
        public bool IsValid => _etag.HasValue || _stateToken != null;

        /// <summary>
        /// Validates if this condition matches the passed entity tag and/or state tokens.
        /// </summary>
        /// <param name="etag">The entity tag.</param>
        /// <param name="stateTokens">The state tokens.</param>
        /// <param name="etagComparer">Comparer for entity tags.</param>
        /// <returns><see langword="true"/> when this condition matches.</returns>
        public bool IsMatch(
            EntityTag? etag,
            IEnumerable<Uri> stateTokens,
            EntityTagComparer? etagComparer = null)
        {
            bool result;

            if (_etag.HasValue)
            {
                if (etag == null)
                {
                    return false;
                }

                var comparer = etagComparer ?? EntityTagComparer.Weak;
                result = comparer.Equals(etag.Value, _etag.Value);
            }
            else if (_stateToken != null)
            {
                var stateToken = _stateToken;
                result = stateTokens.Any(x => x.Equals(stateToken));
            }
            else
            {
                result = false;
            }

            return Not ? !result : result;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            if (_not)
            {
                result.Append("Not ");
            }

            if (_etag.HasValue)
            {
                result.Append('[').Append(_etag).Append(']');
            }
            else if (_stateToken != null)
            {
                result.Append('<').Append(_stateToken).Append('>');
            }
            else
            {
                result.Append("#invalid");
            }

            return result.ToString();
        }

        internal IfHeaderCondition WithNot(bool not)
        {
            if (not == Not)
            {
                return this;
            }

            if (_etag.HasValue)
            {
                return new IfHeaderCondition(not, _etag.Value);
            }

            if (_stateToken != null)
            {
                return new IfHeaderCondition(not, _stateToken);
            }

            return this;
        }
    }
}
