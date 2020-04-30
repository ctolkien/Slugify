using System.Runtime.InteropServices;

namespace Slugify.Core.Pipes {

    /// <summary>
    /// Trims whitespace from the beginning and end of the string, leaving other whitespace intact.
    /// NB: This pipe will cause errors if there are more than 100 consecutive whitespace characters in the input string.
    /// </summary>
    class TrimCharPipe : CharPipe {

        /// <summary>
        /// Storage that will be placed in the context memory
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct Data {
            public bool HasStartedChars;
            public int WhiteSpaceLength;
        }

        readonly int MemLocation;

        unsafe public TrimCharPipe(ref int memLocation) {
            MemLocation = memLocation;
            /// Reserve space not only for the Data struct but also for up to 100 buffer whitespace characters.
            /// Note that this pipe will cause errors if there are more than 100 consecutive whitespace characters in the input string.
            memLocation += sizeof(Data) + 100 * sizeof(char);
        }

        unsafe public override void Write(in Context context, char c) {

            var data = MemoryMarshal.Cast<byte, Data>(context.Memory.Slice(MemLocation, sizeof(Data)));
            var buffer = MemoryMarshal.Cast<byte, char>(context.Memory.Slice(MemLocation + sizeof(Data), 100 * sizeof(char)));

            /// I could comment this better but I didn't. Sorry!!
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

        public override void Complete(in Context context)
            => Continuation.Complete(context);
    }
}
