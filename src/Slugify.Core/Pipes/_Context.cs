using System;

namespace Slugify.Core.Pipes {

    /// <summary>
    /// Encapsulating conversion data within the context allows pipe chains to be used in a multithreaded context.
    /// </summary>
    readonly ref struct Context {

        /// <summary>
        /// Each pipe reserves a section of the memory for storing state-related data.
        /// </summary>
        public readonly Span<byte> Memory;
        
        /// <summary>
        /// Storage for the characters composing the end result.
        /// This is fixed length 1024. It's assumed that no slug will be more than 1024 characters in length.
        /// </summary>
        public readonly Span<char> Result;

        /// <summary>
        /// Allows the <see cref="ResultCharPipe"/> to keep the count of characters it has written.
        /// This is only of length 1.
        /// </summary>
        public readonly Span<int> ResultCount;

        public Context(Span<byte> memory, Span<char> result, Span<int> resultCount) {
            Memory = memory;
            Result = result;
            ResultCount = resultCount;
        }
    }
}
