// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class TagHelperDescriptor : IEquatable<TagHelperDescriptor>
    {
        private IEnumerable<RazorDiagnostic> _allDiagnostics;

        protected TagHelperDescriptor(string kind)
        {
            Kind = kind;
        }

        public string Kind { get; }

        public string Name { get; protected set; }

        public IReadOnlyList<TagMatchingRuleDescriptor> TagMatchingRules { get; protected set; }

        public string AssemblyName { get; protected set; }

        public IReadOnlyList<BoundAttributeDescriptor> BoundAttributes { get; protected set; }

        public IReadOnlyList<AllowedChildTagDescriptor> AllowedChildTags { get; protected set; }

        public string Documentation { get; protected set; }

        public string DisplayName { get; protected set; }

        public string TagOutputHint { get; protected set; }

        public bool CaseSensitive { get; protected set; }

        public IReadOnlyList<RazorDiagnostic> Diagnostics { get; protected set; }

        public IReadOnlyDictionary<string, string> Metadata { get; protected set; }

        // Hoisted / cached metadata
        private int? _hashCode;
        internal bool? IsComponentFullyQualifiedNameMatchCache { get; set; }
        internal bool? IsChildContentTagHelperCache { get; set; }
        internal ParsedTypeInformation? ParsedTypeInfo { get; set; }

        public bool HasErrors
        {
            get
            {
                var allDiagnostics = GetAllDiagnostics();
                var errors = allDiagnostics.Any(diagnostic => diagnostic.Severity == RazorDiagnosticSeverity.Error);

                return errors;
            }
        }

        public virtual IEnumerable<RazorDiagnostic> GetAllDiagnostics()
        {
            if (_allDiagnostics == null)
            {
                var allowedChildTagDiagnostics = AllowedChildTags.SelectMany(childTag => childTag.Diagnostics);
                var attributeDiagnostics = BoundAttributes.SelectMany(attribute => attribute.Diagnostics);
                var ruleDiagnostics = TagMatchingRules.SelectMany(rule => rule.GetAllDiagnostics());
                var combinedDiagnostics = allowedChildTagDiagnostics
                    .Concat(attributeDiagnostics)
                    .Concat(ruleDiagnostics)
                    .Concat(Diagnostics);
                _allDiagnostics = combinedDiagnostics.ToArray();
            }

            return _allDiagnostics;
        }

        public override string ToString()
        {
            return DisplayName ?? base.ToString();
        }

        public bool Equals(TagHelperDescriptor other)
        {
            return TagHelperDescriptorComparer.Default.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TagHelperDescriptor);
        }

        public override int GetHashCode()
        {
            // TagHelperDescriptors are immutable instances and it should be safe to cache it's hashes once.
            _hashCode ??= TagHelperDescriptorComparer.Default.GetHashCode(this);
            return _hashCode.Value;
        }

        internal readonly struct ParsedTypeInformation
        {
            public ParsedTypeInformation(bool success, StringSegment @namespace, StringSegment typeName)
            {
                Success = success;
                Namespace = @namespace;
                TypeName = typeName;
            }

            public bool Success { get; }
            public StringSegment Namespace { get; }
            public StringSegment TypeName { get; }
        }
    }
}
