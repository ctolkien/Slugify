using Slugify.Core.Pipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Slugify.Core {

    public class PipesSlugHelper : ISlugHelper {

        readonly SlugHelper.Config Config;
        readonly CharPipe StartPipe;
        readonly ResultCharPipe ResultPipe;
        readonly int MemorySize;

        public PipesSlugHelper() : this(new SlugHelper.Config()) { }

        public PipesSlugHelper(SlugHelper.Config config) {
            Config = config ?? throw new ArgumentNullException(nameof(config));

            /// To reduce allocations and increase speed, it was important to come up with a design that 
            /// would only build the pipeline once, and re-use it. 
            /// The challenge was making it work multi-threaded. 
            /// Creating a context object and passing it to the pipeline with each character helped achieve that.
            Build(out StartPipe, out ResultPipe, out MemorySize);
        }

        void Build(out CharPipe startPipe, out ResultCharPipe resultPipe, out int memorySize) {
            var memLocation = 0; // memlocation helps us know how much "scratch" workspace memory will need to be provided with each context.
            var currentPipe = startPipe = new DiacriticRemoverPipe();

            if (Config.TrimWhitespace)
                currentPipe = currentPipe.Continue(new TrimCharPipe(ref memLocation));

            currentPipe = currentPipe.Continue(new DeniedCharacterRemovalPipe());

            foreach (var kvp in Config.StringReplacements)
                currentPipe = currentPipe.Continue(new ReplaceCharPipe(kvp.Key, kvp.Value, ref memLocation));

            if (Config.CollapseWhiteSpace)
                currentPipe = currentPipe.Continue(new CollapseWhiteSpacePipe(ref memLocation));

            if (Config.CollapseDashes)
                currentPipe = currentPipe.Continue(new CollapseDashesPipe(ref memLocation));

            if (Config.ForceLowerCase)
                currentPipe = currentPipe.Continue(new ToLowerCharPipe());

            resultPipe = currentPipe.Continue(new ResultCharPipe());
            memorySize = memLocation;
        }

        public string GenerateSlug(string inputString) {

            /// This is the first step of two steps required for removing diacritics.
            /// The second step is implemented in the first char pipe.
            if (!inputString.IsNormalized(NormalizationForm.FormD))
                inputString = inputString.Normalize(NormalizationForm.FormD);

            /// We know exactly how much working memory will be needed by the pipeline, because that was figured out during the 
            /// build in the constructor.
            Span<byte> memory = MemorySize <= 1024 ? stackalloc byte[MemorySize] : new byte[MemorySize];

            /// But we have to take a guess (1024) at the maximum length of the result.
            Span<char> result = stackalloc char[1024];
            Span<int> resultCount = stackalloc int[1];

            var context = new Context(memory, result, resultCount);

            foreach (var c in inputString)
                StartPipe.Write(context, c);

            StartPipe.Complete(context);

            /// Thanks netstandard2.1 for providing this constructor!
            return new string(result.Slice(0, resultCount[0]));
        }
    }
}
