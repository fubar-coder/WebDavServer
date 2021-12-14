// <copyright file="IfHeaderTaggedList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// The <c>Tagged-list</c> of the <c>If</c> header.
    /// </summary>
    public class IfHeaderTaggedList : IIfHeaderList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IfHeaderTaggedList"/> class.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="lists">The lists for this header.</param>
        public IfHeaderTaggedList(
            string reference,
            IReadOnlyCollection<IReadOnlyCollection<IfHeaderCondition>> lists)
        {
            Reference = reference;
            Lists = lists;
        }

        /// <summary>
        /// Gets the reference.
        /// </summary>
        public string Reference { get; }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        public IReadOnlyCollection<IReadOnlyCollection<IfHeaderCondition>> Lists { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append('<').Append(Reference).Append('>');

            foreach (var list in Lists)
            {
                result.Append(' ');
                var isFirstCondition = true;
                foreach (var condition in list)
                {
                    if (isFirstCondition)
                    {
                        isFirstCondition = false;
                    }
                    else
                    {
                        result.Append(' ');
                    }

                    result.Append('(').Append(condition).Append(')');
                }
            }

            return result.ToString();
        }
    }
}
