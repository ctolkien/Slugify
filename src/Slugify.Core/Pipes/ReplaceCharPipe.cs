using System.Runtime.InteropServices;

namespace Slugify.Core.Pipes {

    /// <summary>
    /// Replaces the "search" string with the "replace" string. 
    /// For a replacement to occur, the input strings must be exactly the same as the search string, including case.
    /// </summary>
    class ReplaceCharPipe : CharPipe {

        readonly char[] Search;
        readonly char[] Replace;
        readonly int MemLocation;

        public ReplaceCharPipe(string search, string replace, ref int memLocation) {
            Search = search.ToCharArray();
            Replace = replace.ToCharArray();

            MemLocation = memLocation;
            memLocation += sizeof(int); // We are just storing a single int in the context memory.
        }

        public override void Write(in Context context, char c) {

            /// The single int we store is the count of characters that have matched the "search" string already.
            /// An interesting aspect of this implemenation is that we don't need a buffer stored in the context memory,
            /// because if we're only buffering what matches the search string, we can use the search string as the buffer itself.
            var matchCount = MemoryMarshal.Cast<byte, int>(context.Memory.Slice(MemLocation));

            /// Check if we can buffer characters that match the search string.
            if (c == Search[matchCount[0]]) {
                matchCount[0]++;
                /// Check if we have made the entire match
                if (matchCount[0] == Search.Length) {
                    /// Write the replacement string
                    for (var i = 0; i < Replace.Length; i++)
                        Continuation.Write(context, Replace[i]);
                    /// And reset the buffer
                    matchCount[0] = 0;
                }
            } else {
                /// No match. Write whatever we have buffered and reset the buffer count.
                for (var i = 0; i < matchCount[0]; i++)
                    Continuation.Write(context, Search[i]);
                matchCount[0] = 0;
                /// And write the incoming character, of course.
                Continuation.Write(context, c);
            }
        }

        public override void Complete(in Context context) {
            /// Write any remaining buffered characters before forwarding the completion signal.
            var matchCount = MemoryMarshal.Cast<byte, int>(context.Memory.Slice(MemLocation));
            for (var i = 0; i < matchCount[0]; i++) {
                Continuation.Write(context, Search[i]);
            }
            Continuation.Complete(context);
        }
    }
}
