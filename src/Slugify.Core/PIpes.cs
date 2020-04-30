using System;
using System.Collections.Generic;
using System.Text;

namespace Slugify.Core {

    abstract class CharPipe {

        protected CharPipe Continuation;

        public T Continue<T>(T continuation) where T : CharPipe {
            Continuation = continuation;
            return continuation;
        }

        public abstract void Write(char c);
        public abstract void Complete();
    }

    class NoOpCharPipe : CharPipe {
        public override void Write(char c) => Continuation.Write(c);
        public override void Complete() => Continuation.Complete();
    }

    class ToLowerCharPipe : CharPipe {
        public override void Write(char c) => Continuation.Write(char.ToLower(c));
        public override void Complete() => Continuation.Complete();
    }

    class ReplaceCharPipe : CharPipe {

        readonly char[] Search;
        readonly char[] Replace;

        int matchCount = 0;

        public ReplaceCharPipe(string search, string replace) {
            Search = search.ToCharArray();
            Replace = replace.ToCharArray();
        }
        public override void Write(char c) {
            if (c == Search[matchCount]) {
                matchCount++;
                if (matchCount == Search.Length) {
                    for (var i = 0; i < Replace.Length; i++)
                        Continuation.Write(Replace[i]);
                    matchCount = 0;
                }
            } else {
                for (var i = 0; i < matchCount; i++)
                    Continuation.Write(Search[i]);
                Continuation.Write(c);
                matchCount = 0;
            }
        }
        public override void Complete() {
            for (var i = 0; i < matchCount; i++) {
                Continuation.Write(Search[i]);
            }
            Continuation.Complete();
        }
    }

    class TrimCharPipe : CharPipe {
        readonly List<char> WhiteSpaceBuffer = new List<char>();
        bool hasStartedChars = false;
        public override void Write(char c) {
            if (hasStartedChars) {
                if (char.IsWhiteSpace(c)) {
                    WhiteSpaceBuffer.Add(c);
                } else {
                    foreach (var c2 in WhiteSpaceBuffer)
                        Continuation.Write(c2);
                    WhiteSpaceBuffer.Clear();
                    Continuation.Write(c);
                }
            } else {
                if (!char.IsWhiteSpace(c)) {
                    Continuation.Write(c);
                    hasStartedChars = true;
                }
            }
        }
        public override void Complete() => Continuation.Complete();
    }

    class ResultCharPipe : CharPipe {
        readonly StringBuilder sb = new StringBuilder();
        public override void Write(char c) => sb.Append(c);
        public override void Complete() => Continuation.Complete();
        public string GetResult() => sb.ToString();
    }
}
