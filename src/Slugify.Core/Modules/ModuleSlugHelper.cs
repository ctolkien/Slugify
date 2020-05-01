using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Slugify.Core.Modules {
    public class ModuleSlugHelper : ISlugHelper {

        readonly SlugHelper.Config Config;
        readonly List<Module> Modules;

        public ModuleSlugHelper() : this(new SlugHelper.Config()) { }

        public ModuleSlugHelper(SlugHelper.Config config) {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Modules = Build();
        }

        List<Module> Build() {
            var modules = new List<Module>();
            modules.Add(new DiacriticRemovalModule());
            modules.Add(new WhiteSpaceConversionModule());
            modules.Add(new DeniedCharacterRemovalModule());
            if (Config.CollapseDashes)
                modules.Add(new DashCollapserModule());
            if (Config.TrimWhitespace)
                modules.Add(new DashTrimmerModule());
            if (Config.ForceLowerCase)
                modules.Add(new ToLowerCaseModule());
            foreach (var kvp in Config.StringReplacements)
                modules.Add(new ReplaceModule(kvp.Key, kvp.Value));
            return modules;
        }

        public string GenerateSlug(string inputString) {

            /// This is the first step of two steps required for removing diacritics.
            /// The second step is implemented in the first module, DiacriticRemovalModule.
            if (!inputString.IsNormalized(NormalizationForm.FormD))
                inputString = inputString.Normalize(NormalizationForm.FormD);

            Span<char> input = inputString.Length <= 1024 ? stackalloc char[inputString.Length] : new char[inputString.Length];
            Span<char> output = inputString.Length <= 1024 ? stackalloc char[inputString.Length] : new char[inputString.Length];
            inputString.AsSpan().CopyTo(input);
            var context = new Context(input, output);
            foreach (var module in Modules)
                module.Process(ref context);
            return new string(context.Output);
        }

    }
}
