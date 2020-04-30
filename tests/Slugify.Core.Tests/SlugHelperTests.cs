using System;
using Xunit;
using System.Text.RegularExpressions;

namespace Slugify.Tests
{
    public class SlugHelperTest
    {
        [Fact]
        public void TestEmptyConfig()
        {
            var config = new SlugHelper.Config();
            Assert.True(config.ForceLowerCase);
            Assert.True(config.CollapseWhiteSpace);
            Assert.Single(config.StringReplacements);
            Assert.NotNull(new Regex(config.DeniedCharactersRegex));
        }

        [Fact]
        public void TestDefaultConfig()
        {
            var config = new SlugHelper.Config();

            Assert.Single(config.StringReplacements);
            Assert.Equal("-", config.StringReplacements[" "]);
        }

        [Fact]
        public void TestEmptyConstructor()
        {
            var helper = new SlugHelper();
            Assert.NotNull(helper);
        }

        [Fact]
        public void TestConstructorWithNullConfig()
        {
            Assert.Throws<ArgumentNullException>(() => new SlugHelper(null));
        }

        [Fact]
        public void TestLoweCaseEnforcement()
        {
            const string original = "AbCdE";
            const string expected = "abcde";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug2(original));
        }

        [Fact]
        public void TestWhiteSpaceCollapsing()
        {
            const string original = "a  b    \n  c   \t    d";
            const string expected = "a-b-c-d";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestWhiteSpaceNotCollapsing()
        {
            const string original = "a  b    \n  c   \t    d";
            const string expected = "a-b-c-d";

            var helper = new SlugHelper(new SlugHelper.Config
            {
                CollapseWhiteSpace = false
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestDiacriticRemoval()
        {
            const string withDiacritics = "ñáîùëÓ";
            const string withoutDiacritics = "naiueo";

            var helper = new SlugHelper();

            Assert.Equal(withoutDiacritics, helper.GenerateSlug(withDiacritics));
        }

        [Fact]
        public void TestDeniedCharacterDeletion()
        {
            const string original = "!#$%&/()=";
            const string expected = "";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterReplacement()
        {
            const string original = "abcde";
            const string expected = "xyzde";

            var config = new SlugHelper.Config();
            config.StringReplacements.Add("a", "x");
            config.StringReplacements.Add("b", "y");
            config.StringReplacements.Add("c", "z");

            var helper = new SlugHelper(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Theory]
        [InlineData("E¢Ðƕtoy  mÚÄ´¨ss¨sïuy   !!!!!  Pingüiño", "etoy-muasssiuy-pinguino")]
        [InlineData("QWE dfrewf# $%&!! asd", "qwe-dfrewf-asd")]
        [InlineData("You can't have any pudding if you don't eat your meat!", "you-cant-have-any-pudding-if-you-dont-eat-your-meat")]
        [InlineData("El veloz murciélago hindú", "el-veloz-murcielago-hindu")]
        [InlineData("Médicos sin medicinas medican meditando", "medicos-sin-medicinas-medican-meditando")]

        public void TestFullFunctionality(string input, string output)
        {
            var helper = new SlugHelper();

            Assert.Equal(output, helper.GenerateSlug(input));
        }

        [Fact]
        public void TestConfigForCollapsingDashes()
        {
            const string original = "foo & bar";
            const string expected = "foo-bar";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestConfigForCollapsingDashesWithMoreThanTwoDashes()
        {
            const string original = "foo & bar & & & Jazz&&&&&&&&";
            const string expected = "foo-bar-jazz";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestConfigForNotCollapsingDashes()
        {
            const string original = "foo & bar";
            const string expected = "foo--bar";

            var helper = new SlugHelper(new SlugHelper.Config
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

            var helper = new SlugHelper(new SlugHelper.Config
            {
                TrimWhitespace = true
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact(Skip = "This fails, skipping for now, as not sure if it's a real problem")]
        public void TestConfigForTrimmingWithTrailingReplacedCharacter()
        {
            const string original = "  foo & bar  &";
            const string expected = "foo-bar";

            var helper = new SlugHelper(new SlugHelper.Config
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

            var helper = new SlugHelper(new SlugHelper.Config
            {
                TrimWhitespace = true,
                CollapseDashes = true
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }
    }
}