// <copyright file="IfHeaderEvaluator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.Preconditions
{
    /// <summary>
    /// Precondition evaluation around the <c>If</c> header.
    /// </summary>
    public static class IfHeaderEvaluator
    {
        /// <summary>
        /// Evaluates a single condition.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="information">The resource information.</param>
        /// <returns><see langword="true"/> when the condition evaluates successfully.</returns>
        public static bool Evaluate(
            IfHeaderCondition condition,
            IResourceInformation information)
        {
            if (condition.ETag.HasValue)
            {
                var result = information.HasETag(condition.ETag.Value);
                return condition.Not ? !result : result;
            }

            if (condition.StateToken != null)
            {
                var result = information.HasStateToken(condition.StateToken);
                return condition.Not ? !result : result;
            }

            return false;
        }

        /// <summary>
        /// Evaluates multiple conditions.
        /// </summary>
        /// <param name="conditions">The conditions to evaluate.</param>
        /// <param name="information">The resource information.</param>
        /// <returns><see langword="true"/> when all conditions evaluates successfully.</returns>
        public static bool Evaluate(
            IEnumerable<IfHeaderCondition> conditions,
            IResourceInformation information)
        {
            return conditions.All(condition => Evaluate(condition, information));
        }

        /// <summary>
        /// Evaluates multiple condition lists.
        /// </summary>
        /// <param name="conditionLists">The condition lists to evaluate.</param>
        /// <param name="information">The resource information.</param>
        /// <returns><see langword="true"/> when at least one condition list evaluates successfully.</returns>
        public static bool Evaluate(
            IEnumerable<IEnumerable<IfHeaderCondition>> conditionLists,
            IResourceInformation information)
        {
            return conditionLists.Any(conditions => Evaluate(conditions, information));
        }

        /// <summary>
        /// Evaluates a single <see cref="IfHeaderNoTagList"/>.
        /// </summary>
        /// <param name="entry">The entry to evaluate.</param>
        /// <param name="information">The resource information.</param>
        /// <returns><see langword="true"/> when the entry evaluates successfully.</returns>
        public static bool Evaluate(
            IfHeaderNoTagList entry,
            IResourceInformation information)
        {
            return Evaluate(entry.List, information);
        }

        /// <summary>
        /// Evaluates a single <see cref="IfHeaderTaggedList"/>.
        /// </summary>
        /// <param name="entry">The entry to evaluate.</param>
        /// <param name="information">The resource information.</param>
        /// <returns><see langword="true"/> when the entry evaluates successfully.</returns>
        public static bool Evaluate(
            IfHeaderTaggedList entry,
            IResourceInformation information)
        {
            return Evaluate(entry.Lists, information);
        }

        /// <summary>
        /// Evaluates a <see cref="IfHeader"/>.
        /// </summary>
        /// <param name="header">The header to evaluate.</param>
        /// <param name="informationAccessor">Accessor for the resource information.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><see langword="true"/> when the header evaluates successfully.</returns>
        public static async ValueTask<bool> EvaluateAsync(
            IfHeader header,
            IResourceInformationAccessor informationAccessor,
            CancellationToken cancellationToken = default)
        {
            if (header.IsTaggedList)
            {
                foreach (var taggedList in header.TaggedLists)
                {
                    var information = await informationAccessor.GetResourceInformationAsync(
                            taggedList.Reference,
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (Evaluate(taggedList, information))
                    {
                        return true;
                    }
                }
            }
            else
            {
                var information = await informationAccessor.GetRequestInformationAsync(cancellationToken)
                    .ConfigureAwait(false);
                return header.NoTagLists.Any(noTagList => Evaluate(noTagList, information));
            }

            return false;
        }
    }
}
