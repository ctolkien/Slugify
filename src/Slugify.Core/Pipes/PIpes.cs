using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Slugify.Core.Pipes {

    abstract class CharPipe {

        protected CharPipe Continuation;

        public T Continue<T>(T continuation) where T : CharPipe {
            Continuation = continuation;
            return continuation;
        }

        public abstract void Write(in Context context, char c);
        public abstract void Complete(in Context context);
    }

    readonly ref struct Context {
        public readonly Span<byte> Memory;
        public readonly Span<char> Result;
        public readonly Span<int> ResultCount;
        public Context(Span<byte> memory, Span<char> result, Span<int> resultCount) {
            Memory = memory;
            Result = result;
            ResultCount = resultCount;
        }
    }

    class DiacriticRemoverPipe : CharPipe {
        public override void Write(in Context context, char c) {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                Continuation.Write(context, c);
        }
        public override void Complete(in Context context) => Continuation.Complete(context);
    }

    class DeniedCharacterRemovalPipe : CharPipe {
        public override void Write(in Context context, char c) {

            if (char.IsWhiteSpace(c)) {
                Continuation.Write(context, ' ');
            } else {
                var write = (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c == '.')
                    || (c == '_')
                    || (c == '-');

                if (write)
                    Continuation.Write(context, c);
            }
        }
        public override void Complete(in Context context) => Continuation.Complete(context);
    }


    class ToLowerCharPipe : CharPipe {
        public override void Write(in Context context, char c) => Continuation.Write(context, char.ToLower(c));
        public override void Complete(in Context context) => Continuation.Complete(context);
    }

    class ReplaceCharPipe : CharPipe {

        readonly char[] Search;
        readonly char[] Replace;
        readonly int MemLocation;

        public ReplaceCharPipe(string search, string replace, ref int memLocation) {
            Search = search.ToCharArray();
            Replace = replace.ToCharArray();
            MemLocation = memLocation;
            memLocation += sizeof(int);
        }

        public override void Write(in Context context, char c) {

            var matchCount = MemoryMarshal.Cast<byte, int>(context.Memory.Slice(MemLocation));

            if (c == Search[matchCount[0]]) {
                matchCount[0]++;
                if (matchCount[0] == Search.Length) {
                    for (var i = 0; i < Replace.Length; i++)
                        Continuation.Write(context, Replace[i]);
                    matchCount[0] = 0;
                }
            } else {
                for (var i = 0; i < matchCount[0]; i++)
                    Continuation.Write(context, Search[i]);
                Continuation.Write(context, c);
                matchCount[0] = 0;
            }
        }

        public override void Complete(in Context context) {
            var matchCount = MemoryMarshal.Cast<byte, int>(context.Memory.Slice(MemLocation));
            for (var i = 0; i < matchCount[0]; i++) {
                Continuation.Write(context, Search[i]);
            }
            Continuation.Complete(context);
        }
    }

    class TrimCharPipe : CharPipe {

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct Data {
            public bool HasStartedChars;
            public int WhiteSpaceLength;
        }

        readonly int MemLocation;

        unsafe public TrimCharPipe(ref int memLocation) {
            MemLocation = memLocation;
            memLocation += sizeof(Data) + 100 * sizeof(char);
        }

        unsafe public override void Write(in Context context, char c) {
            var data = MemoryMarshal.Cast<byte, Data>(context.Memory.Slice(MemLocation, sizeof(Data)));
            var buffer = MemoryMarshal.Cast<byte, char>(context.Memory.Slice(MemLocation + sizeof(Data), 100 * sizeof(char)));

            if (data[0].HasStartedChars) {
                if (char.IsWhiteSpace(c)) {
                    buffer[data[0].WhiteSpaceLength++] = c;
                } else {
                    for (var i = 0; i < data[0].WhiteSpaceLength; i++) {
                        Continuation.Write(context, buffer[i]);
                    }
                    data[0].WhiteSpaceLength = 0;
                    Continuation.Write(context, c);
                }
            } else {
                if (!char.IsWhiteSpace(c)) {
                    Continuation.Write(context, c);
                    data[0].HasStartedChars = true;
                }
            }
        }
        public override void Complete(in Context context) => Continuation.Complete(context);
    }

    class CollapseWhiteSpacePipe : CharPipe {
        readonly int MemLocation;
        public CollapseWhiteSpacePipe(ref int memLocation) {
            MemLocation = memLocation;
            memLocation += sizeof(bool);
        }
        public override void Write(in Context context, char c) {
            var isWhitespace = MemoryMarshal.Cast<byte, bool>(context.Memory.Slice(MemLocation));
            if (char.IsWhiteSpace(c)) {
                isWhitespace[0] = true;
            } else {
                if (isWhitespace[0]) {
                    Continuation.Write(context, '-');
                    isWhitespace[0] = false;
                }
                Continuation.Write(context, c);
            }
        }
        public override void Complete(in Context context) {
            Continuation.Complete(context);
        }
    }

    class CollapseDashesPipe : CharPipe {
        readonly int MemLocation;
        public CollapseDashesPipe(ref int memLocation) {
            MemLocation = memLocation;
            memLocation += sizeof(bool);
        }
        public override void Write(in Context context, char c) {
            var isDash = MemoryMarshal.Cast<byte, bool>(context.Memory.Slice(MemLocation));
            if (c == '-') {
                if (!isDash[0]) {
                    Continuation.Write(context, '-');
                    isDash[0] = true;
                }
            } else {
                isDash[0] = false;
                Continuation.Write(context, c);
            }
        }
        public override void Complete(in Context context) => Continuation.Complete(context);
    }

    class ResultCharPipe : CharPipe {
        public override void Write(in Context context, char c) {
            context.Result[context.ResultCount[0]++] = c;
        }
        public override void Complete(in Context context) { }
    }
}
