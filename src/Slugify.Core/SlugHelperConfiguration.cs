using System;
using System.Collections.Generic;

namespace Slugify
{
    /// <summary>
    /// Used to configure the a <see cref="SlugHelper"/> instance
    /// </summary>
    public class SlugHelperConfiguration
    {
        // TODO: Implement a source generator so this can be done at compile time :)
        private static readonly char[] s_allowedChars =
            ("abcdefghijklmnopqrstuvwxyz" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "0123456789" +
            "-._").ToCharArray();

        private readonly HashSet<char> _allowedChars = new HashSet<char>(s_allowedChars);

        public Dictionary<string, string> StringReplacements { get; set; } = new Dictionary<string, string>
                {
                    { " ", "-" }
                };

        public bool ForceLowerCase { get; set; } = true;
        public bool CollapseWhiteSpace { get; set; } = true;
        /// <summary>
        /// Note: Setting this property will stop the AllowedChars feature from being used
        /// </summary>
        public string DeniedCharactersRegex { get; set; }
        public HashSet<char> AllowedChars
        {
            get
            {
                return DeniedCharactersRegex == null ? _allowedChars : throw new InvalidOperationException("After setting DeniedCharactersRegex the AllowedChars feature cannot be used.");
            }
        }
        public bool CollapseDashes { get; set; } = true;
        public bool TrimWhitespace { get; set; } = true;
    }
}

