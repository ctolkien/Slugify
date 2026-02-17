using System;
using System.Globalization;

using Xunit;

namespace Slugify.Tests;

public class SlugHelperTest
{
    private static ISlugHelper Create() => Create(new SlugHelperConfiguration());
    private static ISlugHelper Create(SlugHelperConfiguration config) => new SlugHelper(config);

    [Fact]
    public void TestEmptyConfig()
    {
        var config = new SlugHelperConfiguration();
        Assert.True(config.ForceLowerCase);

        Assert.Single(config.StringReplacements);
        Assert.Null(config.DeniedCharactersRegex);
        Assert.NotEmpty(config.AllowedCharacters);
    }

    [Fact]
    public void TestDeniedCharacterConfig()
    {
        var config = new SlugHelperConfiguration
        {
            DeniedCharactersRegex = new System.Text.RegularExpressions.Regex(string.Empty)
        };

        Assert.Throws<InvalidOperationException>(() => config.AllowedCharacters);
    }

    [Fact]
    public void TestDefaultConfig()
    {
        var config = new SlugHelperConfiguration();

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

    public static TheoryData<ISlugHelper> GenerateStandardSluggers => new()
    {
        { Create() }
    };

    public static TheoryData<ISlugHelper> GenerateNonAsciiSluggers => new()
    {
        { Create(new SlugHelperConfiguration() { SupportNonAsciiLanguages = true }) }
    };

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestLowerCaseEnforcement(ISlugHelper helper)
    {
        const string original = "AbCdE";
        const string expected = "abcde";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestWhiteSpaceCollapsing(ISlugHelper helper)
    {
        const string original = "a  b    \n  c   \t    d";
        const string expected = "a-b-c-d";

        helper.Config.CollapseDashes = true;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestDiacriticRemoval(ISlugHelper helper)
    {
        const string withDiacritics = "ñáîùëÓ";
        const string withoutDiacritics = "naiueo";

        Assert.Equal(withoutDiacritics, helper.GenerateSlug(withDiacritics));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestDeniedCharacterDeletion(ISlugHelper helper)
    {
        const string original = "!#$%&/()=";
        const string expected = "";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestDeniedCharacterDeletionCustomized(ISlugHelper helper)
    {
        const string original = "ab!#$%&/()=";
        const string expected = "b$";

        var config = new SlugHelperConfiguration();
        config.AllowedCharacters.Remove('a');
        config.AllowedCharacters.Add('$');
        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }


    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestDeniedCharacterDeletionLegacy(ISlugHelper helper)
    {
        const string original = "!#$%&/()=";
        const string expected = "";

        helper.Config = new SlugHelperConfiguration
        {
            DeniedCharactersRegex = new(@"[^a-zA-Z0-9\-\._]")
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestDeniedCharacterDeletionLegacy2(ISlugHelper helper)
    {
        const string original = "!#$%&/()=";
        const string expected = "!";

        helper.Config = new SlugHelperConfiguration
        {
            DeniedCharactersRegex = new(@"[^!]")
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestDeniedCharacterDeletionLegacy3(ISlugHelper helper)
    {
        const string original = "regular! !slug";
        const string expected = "regular!-!slug";

        helper.Config = new SlugHelperConfiguration
        {
            DeniedCharactersRegex = new(@"[^a-zA-Z0-9\-\._\!]")
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestDeniedCharacterDeletionLegacy4(ISlugHelper helper)
    {
        const string original = "regular! !slug";
        const string expected = "regular-slug";

        helper.Config = new SlugHelperConfiguration
        {
            DeniedCharactersRegex = new(@"[^a-zA-Z0-9\-\._]")
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }


    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestDeniedCharacterDeletionLegacyKeepsAllowedCharacters(ISlugHelper helper)
    {
        const string original = "Abc -123.$1$_x";
        const string expected = "abc-123.1_x";

        helper.Config = new SlugHelperConfiguration
        {
            DeniedCharactersRegex = new(@"[^a-zA-Z0-9\-\._]")
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementWithWhitespace(ISlugHelper helper)
    {
        const string original = "     abcde     ";
        const string expected = "bcde";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("a", " ");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacement(ISlugHelper helper)
    {
        const string original = "abcde";
        const string expected = "xyzde";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("a", "x");
        config.StringReplacements.Add("b", "y");
        config.StringReplacements.Add("c", "z");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterDoubleReplacement(ISlugHelper helper)
    {
        const string original = "a";
        const string expected = "c";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("a", "b");
        config.StringReplacements.Add("b", "c");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterDoubleReplacementReversedOrder(ISlugHelper helper)
    {
        const string original = "a";
        const string expected = "b";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("b", "c");
        config.StringReplacements.Add("a", "b");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementOrdering(ISlugHelper helper)
    {
        const string original = "catdogfish";
        const string expected = "cdf";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("cat", "c");
        config.StringReplacements.Add("catdog", "e");
        config.StringReplacements.Add("dog", "d");
        config.StringReplacements.Add("fish", "f");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementShortening(ISlugHelper helper)
    {
        const string original = "catdogfish";
        const string expected = "cdf";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("cat", "c");
        config.StringReplacements.Add("dog", "d");
        config.StringReplacements.Add("fish", "f");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementLengthening(ISlugHelper helper)
    {
        const string original = "a";
        const string expected = "ccdccdcc";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("a", "bdbdb");
        config.StringReplacements.Add("b", "cc");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementLookBackwards(ISlugHelper helper)
    {
        const string original = "cat";
        const string expected = "at";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("a", "c");
        config.StringReplacements.Add("cc", "a");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementUmlauts(ISlugHelper helper)
    {
        var config = new SlugHelperConfiguration()
        {
            StringReplacements =
            {
                {"ä", "ae" },
                {"ö", "oe" },
                {"ü", "ue" },
                {"ß", "ss" }
            },
        };

        helper.Config = config;
        Assert.Equal("aeoeueaeoeuess", helper.GenerateSlug("äöüÄÖÜß"));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementUmlautsUppercaseSkipped(ISlugHelper helper)
    {
        var config = new SlugHelperConfiguration()
        {
            ForceLowerCase = false,
            StringReplacements =
            {
                {"ä", "ae" },
                {"ö", "oe" },
                {"ü", "ue" },
                {"ß", "ss" }
            },
        };

        helper.Config = config;
        Assert.Equal("aeoeueAOUss", helper.GenerateSlug("äöüÄÖÜß"));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementUmlautsUppercaseReplaced(ISlugHelper helper)
    {
        var config = new SlugHelperConfiguration()
        {
            ForceLowerCase = false,
            StringReplacements =
            {
                {"Ä", "Ae" },
                {"Ö", "Oe" },
                {"Ü", "Ue" },
                {"ä", "ae" },
                {"ö", "oe" },
                {"ü", "ue" },
                {"ß", "ss" }
            },
        };

        helper.Config = config;
        Assert.Equal("aeoeueAeOeUess", helper.GenerateSlug("äöüÄÖÜß"));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestCharacterReplacementDiacritics(ISlugHelper helper)
    {
        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("Å", "AA");
        config.StringReplacements.Add("å", "aa");
        config.StringReplacements.Add("Æ", "AE");
        config.StringReplacements.Add("æ", "ae");
        config.StringReplacements.Add("Ø", "OE");
        config.StringReplacements.Add("ø", "oe");

        helper.Config = config;
        Assert.Equal("aa-aa-ae-ae-oe-oe", helper.GenerateSlug("Å å Æ æ Ø ø"));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestRecursiveReplacement(ISlugHelper helper)
    {
        const string original = "ycdabbadcz";
        const string expected = "yz";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("abba", "");
        config.StringReplacements.Add("cddc", "");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestRecursiveReplacement2(ISlugHelper helper)
    {
        const string original = "yababbabaz";
        const string expected = "yabbaz";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add("abba", "");

        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestEmptyString(ISlugHelper helper)
    {
        const string original = "";
        const string expected = "";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestNullString(ISlugHelper helper)
    {
        Assert.Throws<ArgumentNullException>(() => helper.GenerateSlug(null));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestOnlyWhitespaceString(ISlugHelper helper)
    {
        const string original = "    \t\n\r    ";
        const string expected = "";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }


    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestVeryLongString(ISlugHelper helper)
    {
        string original = new string('a', 10000) + " " + new string('b', 10000);
        string expected = new string('a', 10000) + "-" + new string('b', 10000);

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestStringWithOnlySpecialCharacters(ISlugHelper helper)
    {
        const string original = "!@#$%^&*()_+{}|:<>?~`-=[];',./";
        const string expected = "_-.";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestOnlyDashesInput(ISlugHelper helper)
    {
        const string original = "--------";
        const string expected = "-";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestNoTrimmingWithLeadingAndTrailingSpaces(ISlugHelper helper)
    {
        const string original = "  hello world  ";
        const string expected = "--hello-world--";

        helper.Config = new SlugHelperConfiguration
        {
            TrimWhitespace = false,
            CollapseDashes = false
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestTrimmingEndChars(ISlugHelper helper)
    {
        const string original = "hello._world._";
        const string expected = "hello._world";

        helper.Config = new SlugHelperConfiguration
        {
            TrimEndChars = [ '.', '_' ],
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestTrimmingStartChars(ISlugHelper helper)
    {
        const string original = "._hello._world";
        const string expected = "hello._world";

        helper.Config = new SlugHelperConfiguration
        {
            TrimStartChars = [ '.', '_' ],
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }
    
    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestTrimmingChars(ISlugHelper helper)
    {
        const string original = "._hello._world._";
        const string expected = "hello._world";

        helper.Config = new SlugHelperConfiguration
        {
            TrimChars = [ '.', '_' ],
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestReplacementWithEmptyStringThenCollapsing(ISlugHelper helper)
    {
        const string original = "hello & world";
        const string expected = "hello-world";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Clear();
        config.StringReplacements.Add(" ", "-");
        config.StringReplacements.Add("&", "");
        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateNonAsciiSluggers))]
    public void TestMixedCharacterSets(ISlugHelper helper)
    {
        const string original = "Hello, 你好, Привет, مرحبا, こんにちは!";
        const string expected = "hello-ni-hao-privet-mrhb-konnichiha";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestUrlFriendlyCharacterPreservation(ISlugHelper helper)
    {
        const string original = "file_name.with-special_chars.txt";
        const string expected = "file_name.with-special_chars.txt";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestMultipleConsecutiveReplacements(ISlugHelper helper)
    {
        const string original = "a & b & c & d";
        const string expected = "a-and-b-and-c-and-d";

        var config = new SlugHelperConfiguration();
        config.StringReplacements.Clear();
        config.StringReplacements.Add(" ", "-");
        config.StringReplacements.Add("&", "and");
        helper.Config = config;

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestSlugifyAlreadySlugifiedString(ISlugHelper helper)
    {
        const string original = "already-slugified-string";
        const string expected = "already-slugified-string";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateNonAsciiSluggers))]
    public void TestComplexNormalizationScenario(ISlugHelper helper)
    {
        // Contains combined characters that need normalization
        const string original = "ǰ ǲ Ǵ ǵ";
        const string expected = "j-dz-g-g";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateNonAsciiSluggers))]
    public void TestMathematicalSymbols(ISlugHelper helper)
    {
        const string original = "Area = π × r²";
        const string expected = "area-p-x-r2";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestPotentialXssString(ISlugHelper helper)
    {
        const string original = "<script>alert('xss');</script>";
        const string expected = "scriptalertxssscript";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestSqlInjectionString(ISlugHelper helper)
    {
        const string original = "'; DROP TABLE users; --";
        const string expected = "-drop-table-users-";

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

    [Theory]
    [InlineData("E¢Ðƕtoy  mÚÄ´¨ss¨sïuy   !!!!!  Pingüiño", "ecdhvtoy-muasssiuy-pinguino")]
    [InlineData("QWE dfrewf# $%&!! asd", "qwe-dfrewf-asd")]
    [InlineData("You can't have any pudding if you don't eat your meat!", "you-cant-have-any-pudding-if-you-dont-eat-your-meat")]
    [InlineData("El veloz murciélago hindú", "el-veloz-murcielago-hindu")]
    [InlineData("Médicos sin medicinas medican meditando", "medicos-sin-medicinas-medican-meditando")]
    [InlineData("Você está numa situação lamentável", "voce-esta-numa-situacao-lamentavel")]
    [InlineData("crème brûlée", "creme-brulee")]
    [InlineData("ä ö ü", "a-o-u")]
    [InlineData("ç Ç ğ Ğ ı I i İ ö Ö ş Ş ü Ü", "c-c-g-g-i-i-i-i-o-o-s-s-u-u")]
    [InlineData("Актуални предложения", "aktualni-predlozheniya")]

    public void TestFullNonAsciiFunctionality(string input, string output)
    {
        var helper = Create(new SlugHelperConfiguration() { SupportNonAsciiLanguages = true });

        Assert.Equal(output, helper.GenerateSlug(input));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestConfigForCollapsingDashes(ISlugHelper helper)
    {
        const string original = "foo & bar";
        const string expected = "foo-bar";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestConfigForCollapsingDashesWithMoreThanTwoDashes(ISlugHelper helper)
    {
        const string original = "foo & bar & & & Jazz&&&&&&&&";
        const string expected = "foo-bar-jazz";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestConfigForNotCollapsingDashes(ISlugHelper helper)
    {
        const string original = "foo & bar";
        const string expected = "foo--bar";

        helper.Config = new SlugHelperConfiguration
        {
            CollapseDashes = false
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestConfigForTrimming(ISlugHelper helper)
    {
        const string original = "  foo & bar  ";
        const string expected = "foo-bar";

        helper.Config = new SlugHelperConfiguration
        {
            TrimWhitespace = true
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateStandardSluggers))]
    public void TestHandlingOfUnicodeCharacters(ISlugHelper helper)
    {
        const string original = "unicode ♥ support";
        const string expected = "unicode-support";

        helper.Config = new SlugHelperConfiguration
        {
            TrimWhitespace = true,
            CollapseDashes = true
        };

        Assert.Equal(expected, helper.GenerateSlug(original));
    }


    [Theory]
    [MemberData(nameof(GenerateNonAsciiSluggers))]
    public void TurkishEncoding_NonAscii(ISlugHelper helper)
    {
        var defaultCulture = CultureInfo.CurrentCulture;
        try
        {
            //Set culture to Turkish
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            const string original = "ÇEVRE VE ŞEHİRCİLİK BAKANLIĞI İstanbul Fen İşleri Daire Başkanlığı Özel İdare Müdürlüğü";
            const string expected = "cevre-ve-sehircilik-bakanligi-istanbul-fen-isleri-daire-baskanligi-ozel-idare-mudurlugu";

            Assert.Equal(expected, helper.GenerateSlug(original));
        }
        finally
        {
            //Reset culture
            CultureInfo.CurrentCulture = defaultCulture;
        }
    }

    [Theory]
    [MemberData(nameof(GenerateNonAsciiSluggers))]
    public void TurkishEncodingOfI(ISlugHelper helper)
    {
        var defaultCulture = CultureInfo.CurrentCulture;
        try
        {
            //Set culture to Turkish
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            const string original = "FIFA 18";
            const string expected = "fifa-18";

            Assert.Equal(expected, helper.GenerateSlug(original));
        }
        finally
        {
            //Reset culture
            CultureInfo.CurrentCulture = defaultCulture;
        }
    }

    [Theory]
    [MemberData(nameof(GenerateNonAsciiSluggers))]
    public void Arabic(ISlugHelper helper)
    {
        const string original = "نوشتار فارسی";
        const string expected = "nwshtr-frsy";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [MemberData(nameof(GenerateNonAsciiSluggers))]
    public void Cyrillic(ISlugHelper helper)
    {
        const string original = "Наизусть";
        const string expected = "naizust";

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Theory]
    [InlineData(null, "abcdefghijgklmnopqrstuvwxy", "abcdefghijgklmnopqrstuvwxy")]
    [InlineData(8, "abcdefghijgklmnopqrstuvwxy", "abcdefgh")]
    [InlineData(8, "ab c d e fgh", "ab-c-d-e")]
    [InlineData(7, "ab c d e", "ab-c-d")]
    [InlineData(8, "ab c d ", "ab-c-d")]
    public void MaximumLengthGivenTrimsUnnecessaryChars(int? length, string input, string expected)
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = length
        });
        Assert.Equal(expected, helper.GenerateSlug(input));
    }

    [Fact]
    public void TestsInTheReadme()
    {
        const string original = "Simple,short&quick Example";
        const string expected = "Simple-short-quick-Example";

        // Creating a configuration object
        var config = new SlugHelperConfiguration();

        // Add individual replacement rules
        config.StringReplacements.Add("&", "-");
        config.StringReplacements.Add(",", "-");

        // Keep the casing of the input string
        config.ForceLowerCase = false;


        var helper = Create(config);


        Assert.Equal(expected, helper.GenerateSlug(original));
    }

}