using System;
using Xunit;

namespace Slugify.Tests
{
    public class SlugHelperTest
    {
        private static ISlugHelper Create() => Create(new SlugHelperLegacy.Config());
        private static ISlugHelper Create(SlugHelperLegacy.Config config) => new SlugHelper(config);

        [Fact]
        public void TestEmptyConfig()
        {
            var config = new SlugHelperLegacy.Config();
            Assert.True(config.ForceLowerCase);
            Assert.True(config.CollapseWhiteSpace);
            Assert.Single(config.StringReplacements);
            Assert.Null(config.DeniedCharactersRegex);
            Assert.NotEmpty(config.AllowedChars);
        }

        [Fact]
        public void TestDeniedCharacterConfig()
        {
            var config = new SlugHelperLegacy.Config
            {
                DeniedCharactersRegex = ""
            };

            Assert.Throws<InvalidOperationException>(() => config.AllowedChars);
        }

        [Fact]
        public void TestDefaultConfig()
        {
            var config = new SlugHelperLegacy.Config();

            Assert.Single(config.StringReplacements);
            Assert.Equal("-", config.StringReplacements[" "]);
        }

        [Fact]
        public void TestEmptyConstructor()
        {
            var helper = Create();
            Assert.NotNull(helper);
        }

        [Fact]
        public void TestConstructorWithNullConfig()
        {
            Assert.Throws<ArgumentNullException>(() => Create(null));
        }

        [Fact]
        public void TestLoweCaseEnforcement()
        {
            const string original = "AbCdE";
            const string expected = "abcde";

            var helper = Create();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestWhiteSpaceCollapsing()
        {
            const string original = "a  b    \n  c   \t    d";
            const string expected = "a-b-c-d";

            var helper = Create(new SlugHelperLegacy.Config
            {
                CollapseDashes = false
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestWhiteSpaceNotCollapsing()
        {
            const string original = "a  b    \n  c   \t    d";
            const string expected = "a--b-------c--------d";

            var helper = Create(new SlugHelperLegacy.Config
            {
                CollapseDashes = false,
                CollapseWhiteSpace = false
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestDiacriticRemoval()
        {
            const string withDiacritics = "ñáîùëÓ";
            const string withoutDiacritics = "naiueo";

            var helper = Create();

            Assert.Equal(withoutDiacritics, helper.GenerateSlug(withDiacritics));
        }

        [Fact]
        public void TestDeniedCharacterDeletion()
        {
            const string original = "!#$%&/()=";
            const string expected = "";

            var helper = Create();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestDeniedCharacterDeletionCustomized()
        {
            const string original = "ab!#$%&/()=";
            const string expected = "b$";

            var config = new SlugHelperLegacy.Config();
            config.AllowedChars.Remove('a');
            config.AllowedChars.Add('$');
            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }


        [Fact]
        public void TestDeniedCharacterDeletionLegacy()
        {
            const string original = "!#$%&/()=";
            const string expected = "";

            var helper = Create(new SlugHelperLegacy.Config
            {
                DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._]"
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterReplacementWithWhitespace()
        {
            const string original = "     abcde     ";
            const string expected = "bcde";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("a", " ");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterReplacement()
        {
            const string original = "abcde";
            const string expected = "xyzde";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("a", "x");
            config.StringReplacements.Add("b", "y");
            config.StringReplacements.Add("c", "z");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterDoubleReplacement()
        {
            const string original = "a";
            const string expected = "c";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("a", "b");
            config.StringReplacements.Add("b", "c");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterReplacementOrdering()
        {
            const string original = "catdogfish";
            const string expected = "cdf";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("cat", "c");
            config.StringReplacements.Add("catdog", "e");
            config.StringReplacements.Add("dog", "d");
            config.StringReplacements.Add("fish", "f");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterReplacementShorting()
        {
            const string original = "catdogfish";
            const string expected = "cdf";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("cat", "c");
            config.StringReplacements.Add("dog", "d");
            config.StringReplacements.Add("fish", "f");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterReplacementLengthening()
        {
            const string original = "a";
            const string expected = "ccdccdcc";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("a", "bdbdb");
            config.StringReplacements.Add("b", "cc");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterReplacementLookBackwards()
        {
            const string original = "cat";
            const string expected = "at";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("a", "c");
            config.StringReplacements.Add("cc", "a");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestRecursiveReplacement()
        {
            const string original = "ycdabbadcz";
            const string expected = "yz";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("abba", "");
            config.StringReplacements.Add("cddc", "");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestRecursiveReplacement2()
        {
            const string original = "yababbabaz";
            const string expected = "yabbaz";

            var config = new SlugHelperLegacy.Config();
            config.StringReplacements.Add("abba", "");

            var helper = Create(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Theory]
        [InlineData("E¢Ðƕtoy  mÚÄ´¨ss¨sïuy   !!!!!  Pingüiño", "etoy-muasssiuy-pinguino")]
        [InlineData("QWE dfrewf# $%&!! asd", "qwe-dfrewf-asd")]
        [InlineData("You can't have any pudding if you don't eat your meat!", "you-cant-have-any-pudding-if-you-dont-eat-your-meat")]
        [InlineData("El veloz murciélago hindú", "el-veloz-murcielago-hindu")]
        [InlineData("Médicos sin medicinas medican meditando", "medicos-sin-medicinas-medican-meditando")]
        [InlineData("Você está numa situação lamentável", "voce-esta-numa-situacao-lamentavel")]
        [InlineData("crème brûlée", "creme-brulee")]
        [InlineData("ä ö ü", "a-o-u")]

        public void TestFullFunctionality(string input, string output)
        {
            var helper = Create();

            Assert.Equal(output, helper.GenerateSlug(input));
        }

        [Fact]
        public void TestConfigForCollapsingDashes()
        {
            const string original = "foo & bar";
            const string expected = "foo-bar";

            var helper = Create();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestConfigForCollapsingDashesWithMoreThanTwoDashes()
        {
            const string original = "foo & bar & & & Jazz&&&&&&&&";
            const string expected = "foo-bar-jazz";

            var helper = Create();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestConfigForNotCollapsingDashes()
        {
            const string original = "foo & bar";
            const string expected = "foo--bar";

            var helper = Create(new SlugHelperLegacy.Config
            {
                CollapseDashes = false
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestConfigForTrimming()
        {
            const string original = "  foo & bar  ";
            const string expected = "foo-bar";

            var helper = Create(new SlugHelperLegacy.Config
            {
                TrimWhitespace = true
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestHandlingOfUnicodeCharacters()
        {
            const string original = "unicode ♥ support";
            const string expected = "unicode-support";

            var helper = Create(new SlugHelperLegacy.Config
            {
                TrimWhitespace = true,
                CollapseDashes = true
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }
    }
}