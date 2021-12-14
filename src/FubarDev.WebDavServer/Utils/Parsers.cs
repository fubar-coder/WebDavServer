// <copyright file="Parsers.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using FubarDev.WebDavServer.Model.Headers;

using Parlot;
using Parlot.Fluent;

using static Parlot.Fluent.Parsers;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// Common parsers for WebDAV structures.
    /// </summary>
    public static class Parsers
    {
        private static readonly Parser<string> _resourceTag =
            Terms.Char('<')
                .And(AnyCharBefore(Literals.Char('>')))
                .And(Terms.Char('>').ElseError("Expected '>' (resource tag)"))
                .Then(static _ => _.Item2.Buffer.Substring(_.Item2.Offset, _.Item2.Length).Trim());

        private static readonly Parser<Uri> _codedUrl =
            Terms.Char('<')
                .And(AnyCharBefore(Literals.Char('>')))
                .And(Terms.Char('>').ElseError("Expected '>' (coded url)"))
                .Then(static _ => new Uri(_.Item2.Buffer.Substring(_.Item2.Offset, _.Item2.Length).Trim()));

        private static readonly Parser<string> _quotedString =
            Terms.Char('"').ElseError("Expected a starting quote")
            .And(
                ZeroOrMany(
                    Literals.Pattern(c => c != '"' && c != '\\')
                        .Or(
                            Literals.Char('\\')
                                .And(Literals.Pattern(_ => true, 1, 1))
                                .Then(_ => Unescape(_.Item2.Buffer[_.Item2.Offset])))))
            .And(Literals.Char('"').ElseError("Expected an ending quote"))
            .Then(
                _ => string.Join(
                    string.Empty,
                    _.Item2.Select(t => t.Buffer.Substring(t.Offset, t.Length))));

        private static readonly Parser<EntityTag> _entityTag =
            ZeroOrOne(Terms.Text("W/", true).Then(_ => true))
                .And(_quotedString)
                .Then(static _ => new EntityTag(_.Item1, _.Item2));

        private static readonly Parser<IfHeaderCondition> _entityTagCondition =
            Terms.Char('[')
                .And(_entityTag)
                .And(Terms.Char(']').ElseError("Expected ']' (entity tag condition)"))
                .Then(static _ => new IfHeaderCondition(false, _.Item2));

        private static readonly Parser<IfHeaderCondition> _codedUrlCondition =
            _codedUrl
                .Then(static _ => new IfHeaderCondition(false, _));

        private static readonly Parser<IfHeaderCondition> _condition =
            ZeroOrOne(Terms.Text("Not", true).Then(_ => true))
                .And(_entityTagCondition.Or(_codedUrlCondition))
                .Then(static _ => _.Item2.WithNot(_.Item1));

        private static readonly Parser<List<IfHeaderCondition>> _ifHeaderList =
            Terms.Char('(')
                .And(OneOrMany(_condition))
                .And(Terms.Char(')').ElseError("Expected ')' (if header list)"))
                .Then(static _ => _.Item2);

        private static readonly Parser<IIfHeaderList> _ifHeaderNoTagList =
            _ifHeaderList
                .Then(static _ => (IIfHeaderList)new IfHeaderNoTagList(_));

        private static readonly Parser<IIfHeaderList> _ifHeaderTaggedList =
            _resourceTag.And(OneOrMany(_ifHeaderList))
                .Then(static _ => (IIfHeaderList)new IfHeaderTaggedList(_.Item1, _.Item2));

        private static readonly Parser<List<IIfHeaderList>> _ifHeader =
            OneOrMany(_ifHeaderNoTagList)
                .Or(OneOrMany(_ifHeaderTaggedList))
                .ElseError("Expected '<' (resource tag) or '(' (list)")
                .Eof();

        private static readonly Parser<string> _token =
            Literals.Pattern(ch => !IsSeparator(ch) && !IsControlCharacter(ch), 0)
                .Then(_ => _.Buffer.Substring(_.Offset, _.Length));

        /// <summary>
        /// Parse a token.
        /// </summary>
        public static Parser<string> Token { get; } = _token.Compile();

        /// <summary>
        /// Parse a <see cref="LockTokenHeader"/>.
        /// </summary>
        public static Parser<LockTokenHeader> LockTokenHeader { get; } =
            _codedUrl
                .Then(static _ => new LockTokenHeader(_));

        /// <summary>
        /// Gets the (compiled) parser for an <c>If</c> header.
        /// </summary>
        public static Parser<IfHeader> IfHeader { get; } =
            _ifHeader
                .Then(_ => new IfHeader(_))
                .Compile();

        /// <summary>
        /// Gets the (interpreted) parser for an entity tag.
        /// </summary>
        public static Parser<List<EntityTag>> EntityTags { get; } =
            ZeroOrMany(_entityTag)
                .AndSkip(ZeroOrMany(Literals.WhiteSpace()))
                .Eof()
                .ElseError("Invalid entity tag")
                .Compile();

        private static TextSpan Unescape(char ch) =>
            ch switch
            {
                '"' => new TextSpan("\""),
                _ => new TextSpan(Regex.Unescape($"\\{ch}")),
            };

        private static bool IsSeparator(char ch) =>
            ch is '(' or ')' or '<' or '>' or '@' or ',' or ';' or ':' or '\\' or '"' or '/' or '[' or ']' or '?' or '='
                or '{' or '}' or ' ' or '\t';

        private static bool IsControlCharacter(char ch) =>
            (int)ch is < 32 or 127;
    }
}
