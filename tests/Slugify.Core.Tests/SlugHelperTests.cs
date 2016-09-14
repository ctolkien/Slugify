using System;
using Xunit;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Slugify.Tests
{
    public class SlugHelperTest
    {
        [Fact]
        public void TestEmptyConfig()
        {
            SlugHelper.Config config = new SlugHelper.Config();
            Assert.True(config.ForceLowerCase);
            Assert.True(config.CollapseWhiteSpace);
            Assert.Equal(1, config.StringReplacements.Count);
            Assert.NotNull(new Regex(config.DeniedCharactersRegex));
        }

        [Fact]
        public void TestDefaultConfig()
        {
            SlugHelper.Config config = new SlugHelper.Config();
            
            Assert.Equal(1, config.StringReplacements.Count);
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
            var original = "AbCdE";
            var expected = "abcde";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestWhiteSpaceCollapsing()
        {
            var original = "a  b    \n  c   \t    d";
            var expected = "a-b-c-d";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestWhiteSpaceNotCollapsing()
        {
            var original = "a  b    \n  c   \t    d";
            var expected = "a-b-c-d";

            var helper = new SlugHelper(new SlugHelper.Config
            {
                CollapseWhiteSpace = false
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestDiacriticRemoval()
        {
            var withDiacritics = "ñáîùëÓ";
            var withoutDiacritics = "naiueo";

            var helper = new SlugHelper();

            Assert.Equal(withoutDiacritics, helper.GenerateSlug(withDiacritics));
        }

        [Fact]
        public void TestDeniedCharacterDeletion()
        {
            var original = "!#$%&/()=";
            var expected = "";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestCharacterReplacement()
        {
            var original = "abcde";
            var expected = "xyzde";

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
            var original = "foo & bar";
            var expected = "foo-bar";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestConfigForCollapsingDashesWithMoreThanTwoDashes()
        {
            var original = "foo & bar & & & Jazz&&&&&&&&";
            var expected = "foo-bar-jazz";

            var helper = new SlugHelper();

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestConfigForNotCollapsingDashes()
        {
            var original = "foo & bar";
            var expected = "foo--bar";

            var helper = new SlugHelper(new SlugHelper.Config
            {
                CollapseDashes = false
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestConfigForTrimming()
        {
            var original = "  foo & bar  ";
            var expected = "foo-bar";

            var helper = new SlugHelper(new SlugHelper.Config
            {
                TrimWhitespace = true
            });

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

    }
}