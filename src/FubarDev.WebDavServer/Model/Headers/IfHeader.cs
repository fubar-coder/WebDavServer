// <copyright file="IfHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Class that represents the HTTP <c>If</c> header.
    /// </summary>
    public class IfHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IfHeader"/> class.
        /// </summary>
        /// <param name="lists">The header lists.</param>
        internal IfHeader(IReadOnlyCollection<IIfHeaderList> lists)
        {
            Lists = lists;
        }

        /// <summary>
        /// Gets all condition lists.
        /// </summary>
        public IReadOnlyCollection<IIfHeaderList> Lists { get; }

        /// <summary>
        /// Gets a value indicating whether the <c>If</c> header contains tagged lists.
        /// </summary>
        public bool IsTaggedList => Lists.First() is IfHeaderTaggedList;

        /// <summary>
        /// Gets the tagged lists.
        /// </summary>
        public IEnumerable<IfHeaderTaggedList> TaggedLists => Lists.Cast<IfHeaderTaggedList>();

        /// <summary>
        /// Gets the No-tag lists.
        /// </summary>
        public IEnumerable<IfHeaderNoTagList> NoTagLists => Lists.Cast<IfHeaderNoTagList>();
    }
}
