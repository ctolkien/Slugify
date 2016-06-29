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
            Assert.Equal(1, config.CharacterReplacements.Count);
            Assert.NotNull(new Regex(config.DeniedCharactersRegex));
        }

        [Fact]
        public void TestDefaultConfig()
        {
            KeyValuePair<string, string> defaultReplacement = new KeyValuePair<string, string>(" ","-");
            
            SlugHelper.Config config = new SlugHelper.Config();
            
            Assert.Equal(1, config.CharacterReplacements.Count);
            Assert.Equal("-", config.CharacterReplacements[" "]);
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
            config.CharacterReplacements.Add("a", "x");
            config.CharacterReplacements.Add("b", "y");
            config.CharacterReplacements.Add("c", "z");

            var helper = new SlugHelper(config);

            Assert.Equal(expected, helper.GenerateSlug(original));
        }

        [Fact]
        public void TestFullFunctionality()
        {
            var helper = new SlugHelper();
            var tests = new Dictionary<string, string>();
            
            tests.Add(  "E¢Ðƕtoy  mÚÄ´¨ss¨sïuy   !!!!!  Pingüiño",
                        "etoy-muasssiuy--pinguino");

            tests.Add(  "QWE dfrewf# $%&!! asd",
                        "qwe-dfrewf--asd");
            
            tests.Add(  "You can't have any pudding if you don't eat your meat!",
                        "you-cant-have-any-pudding-if-you-dont-eat-your-meat");

            tests.Add(  "El veloz murciélago hindú",
                        "el-veloz-murcielago-hindu");

            tests.Add(  "Médicos sin medicinas medican meditando",
                        "medicos-sin-medicinas-medican-meditando");

            foreach(KeyValuePair<string, string> test in tests){
                Assert.Equal(test.Value, helper.GenerateSlug(test.Key));
            }
        }

    }
}