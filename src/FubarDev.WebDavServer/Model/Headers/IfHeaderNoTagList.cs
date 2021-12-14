// <copyright file="IfHeaderNoTagList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// The <c>No-tag-list</c> of the <c>If</c> header.
    /// </summary>
    public class IfHeaderNoTagList : IIfHeaderList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IfHeaderNoTagList"/> class.
        /// </summary>
        /// <param name="list">The list of conditions.</param>
        public IfHeaderNoTagList(IReadOnlyCollection<IfHeaderCondition> list)
        {
            List = list;
        }

        public IReadOnlyCollection<IfHeaderCondition> List { get; }

        public override string ToString()
        {
            var result = new StringBuilder();
            var isFirstCondition = true;
            foreach (var condition in List)
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

            return result.ToString();
        }
    }
}
