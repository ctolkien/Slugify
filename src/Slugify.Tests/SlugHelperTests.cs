using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Slugify.Tests
{

    [TestClass]
    public class SlugHelperTest
    {

        [TestMethod]
        public void TestEmptyConfig()
        {
            SlugHelper.Config config = new SlugHelper.Config();
            Assert.IsTrue(config.ForceLowerCase);
            Assert.IsTrue(config.CollapseWhiteSpace);
            Assert.AreEqual(1, config.CharacterReplacements.Count);
            Assert.IsNotNull(new Regex(config.DeniedCharactersRegex));
        }

        [TestMethod]
        public void TestDefaultConfig()
        {
            KeyValuePair<string, string> defaultReplacement = new KeyValuePair<string, string>(" ","-");
            
            SlugHelper.Config config = new SlugHelper.Config();
            
            Assert.AreEqual(1, config.CharacterReplacements.Count);
            Assert.AreEqual("-", config.CharacterReplacements[" "]);
        }

        [TestMethod]
        public void TestEmptyConstructor()
        {
            SlugHelper helper = new SlugHelper();
            Assert.IsNotNull(helper);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void TestConstructorWithNullConfig()
        {
            SlugHelper helper = new SlugHelper(null);
        }

        [TestMethod]
        public void TestLoweCaseEnforcement()
        {
            String original = "AbCdE";
            String expected = "abcde";

            SlugHelper helper = new SlugHelper();

            Assert.AreEqual(expected, helper.GenerateSlug(original));
        }

        [TestMethod]
        public void TestWhiteSpaceCollapsing()
        {
            String original = "a  b    \n  c   \t    d";
            String expected = "a-b-c-d";

            SlugHelper helper = new SlugHelper();

            Assert.AreEqual(expected, helper.GenerateSlug(original));
        }
        
        [TestMethod]
        public void TestDiacriticRemoval()
        {
            String withDiacritics = "ñáîùëÓ";
            String withoutDiacritics = "naiueo";

            SlugHelper helper = new SlugHelper();

            Assert.AreEqual(withoutDiacritics, helper.GenerateSlug(withDiacritics));
        }

        [TestMethod]
        public void TestDeniedCharacterDeletion()
        {
            String original = "!#$%&/()=";
            String expected = "";

            SlugHelper helper = new SlugHelper();

            Assert.AreEqual(expected, helper.GenerateSlug(original));
        }

        [TestMethod]
        public void TestCharacterReplacement()
        {
            String original = "abcde";
            String expected = "xyzde";

            SlugHelper.Config config = new SlugHelper.Config();
            config.CharacterReplacements.Add("a", "x");
            config.CharacterReplacements.Add("b", "y");
            config.CharacterReplacements.Add("c", "z");

            SlugHelper helper = new SlugHelper(config);

            Assert.AreEqual(expected, helper.GenerateSlug(original));
        }

        [TestMethod]
        public void TestFullFunctionality()
        {
            SlugHelper helper = new SlugHelper();
            Dictionary<string, string> tests = new Dictionary<string, string>();
            
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
                Assert.AreEqual(test.Value, helper.GenerateSlug(test.Key));
            }
        }

    }
}