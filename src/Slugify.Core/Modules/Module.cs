using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;

namespace Slugify.Core.Modules {

    ref struct Context {

        public Span<char> Input { get; private set; }
        public Span<char> Output { get; private set; }

        public Context(Span<char> input, Span<char> output) {
            Input = input;
            Output = output;
        }

        public void Update(int newLength) {
            var tmp = Input;
            Input = Output.Slice(0, newLength);
            Output = tmp.Slice(0, newLength);
        }
    }

    abstract class Module {

        public abstract void Process(ref Context context);
    }

    class DiacriticRemovalModule : Module {
        public override void Process(ref Context context) {
            var outputLength = 0;
            for (var i = 0; i < context.Input.Length; i++) {
                if (CharUnicodeInfo.GetUnicodeCategory(context.Input[i]) != UnicodeCategory.NonSpacingMark)
                    context.Output[outputLength++] = context.Input[i];
            }
            context.Update(outputLength);
        }
    }

    class WhiteSpaceConversionModule : Module {
        public override void Process(ref Context context) {
            var outputLength = 0;
            var isWhitespace = false;
            for (var i = 0; i < context.Input.Length; i++) {
                if (char.IsWhiteSpace(context.Input[i])) {
                    if (!isWhitespace) {
                        context.Output[outputLength++] = '-';
                        isWhitespace = true;
                    }
                } else {
                    context.Output[outputLength++] = context.Input[i];
                    isWhitespace = false;
                }
            }
            context.Update(outputLength);
        }
    }

    class DeniedCharacterRemovalModule : Module {
        public override void Process(ref Context context) {
            var outputLength = 0;
            for (var i = 0; i < context.Input.Length; i++) {
                var c = context.Input[i];
                var write = (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= '0' && c <= '9')
                    || (c == '.')
                    || (c == '_')
                    || (c == '-');
                if (write) {
                    context.Output[outputLength++] = c;
                }
            }
            context.Update(outputLength);
        }
    }

    class DashCollapserModule : Module {
        public override void Process(ref Context context) {
            var outputLength = 0;
            var isDash = false;
            for (var i = 0; i < context.Input.Length; i++) {
                if (context.Input[i] == '-') {
                    if (!isDash) {
                        isDash = true;
                        context.Output[outputLength++] = '-';
                    }
                } else {
                    isDash = false;
                    context.Output[outputLength++] = context.Input[i];
                }
            }
            context.Update(outputLength);
        }
    }

    class DashTrimmerModule : Module {
        public override void Process(ref Context context) {
            var outputLength = 0;
            if (context.Input.Length > 0) {
                if (context.Input[0] != '-')
                    context.Output[outputLength++] = context.Input[0];
                if (context.Input.Length > 1) {
                    for (var i = 1; i < context.Input.Length - 1; i++)
                        context.Output[outputLength++] = context.Input[i];
                    if (context.Input[context.Input.Length - 1] != '-')
                        context.Output[outputLength++] = context.Input[context.Input.Length - 1];
                }
            }
            context.Update(outputLength);
        }
    }

    class ToLowerCaseModule : Module {
        public override void Process(ref Context context) {
            for (var i = 0; i < context.Input.Length; i++)
                context.Output[i] = char.ToLower(context.Input[i]);
            context.Update(context.Input.Length);
        }
    }

    class ReplaceModule : Module {
        readonly char[] Search;
        readonly char[] Replace;

        public ReplaceModule(string search, string replace) {
            Search = search.ToCharArray();
            Replace = replace.ToCharArray();
        }

        public override void Process(ref Context context) {
            do {
                var outputLength = 0;
                var matchCount = 0;

                for (var i = 0; i < context.Input.Length; i++) {
                    if (context.Input[i] == Search[matchCount]) {
                        matchCount++;
                        if (matchCount == Search.Length) {
                            for (var j = 0; j < Replace.Length; j++)
                                context.Output[outputLength++] = Replace[j];
                            matchCount = 0;
                        }
                    } else {
                        if (matchCount > 0) {
                            for (var j = 0; i < matchCount; j++)
                                context.Output[outputLength++] = Search[j];
                            matchCount = 0;
                        }
                        context.Output[outputLength++] = context.Input[i];
                    }
                }

                for (var i = 0; i < matchCount; i++)
                    context.Output[outputLength++] = Search[i];

                context.Update(outputLength);
            } while (!Compare(context.Input, context.Output));
        }

        static bool Compare(Span<char> a, Span<char> b) {
            if (a.Length != a.Length) return false;
            for (var i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}
