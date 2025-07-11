using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

    [Theory]
    [InlineData("The very long name liga", 12)]
    [InlineData("The very long name liga (W)", 12)]
    [InlineData("The very long name liga (M)", 12)]
    [InlineData("abcdefghijklmnopqrstuvwxy", 15)]
    public void EnableHashedShorteningCreatesUniqueResults(string input, int maxLength)
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = maxLength,
            EnableHashedShortening = true
        });
        
        var result = helper.GenerateSlug(input);
        
        // Should be within the maximum length
        Assert.True(result.Length <= maxLength);
        
        // Should end with a dash followed by 2 hex characters (hash)
        Assert.True(result.Length >= 3); // At least "X-YZ"
        Assert.Equal('-', result[result.Length - 3]);
        
        // Last 2 characters should be valid hex
        var hash = result.Substring(result.Length - 2);
        Assert.True(hash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
    }

    [Fact]
    public void EnableHashedShorteningCreatesDifferentHashesForDifferentInputs()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 12,
            EnableHashedShortening = true
        });
        
        var result1 = helper.GenerateSlug("The very long name liga");
        var result2 = helper.GenerateSlug("The very long name liga (W)");
        var result3 = helper.GenerateSlug("The very long name liga (M)");
        
        // All should be different
        Assert.NotEqual(result1, result2);
        Assert.NotEqual(result1, result3);
        Assert.NotEqual(result2, result3);
        
        // All should have different hash suffixes
        var hash1 = result1.Substring(result1.Length - 2);
        var hash2 = result2.Substring(result2.Length - 2);
        var hash3 = result3.Substring(result3.Length - 2);
        
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
    }

    [Fact]
    public void EnableHashedShorteningWithTooShortMaxLengthFallsBackToTruncation()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 3,
            EnableHashedShortening = true
        });
        
        var result = helper.GenerateSlug("test input");
        
        // Should fallback to simple truncation
        Assert.Equal(3, result.Length);
        Assert.Equal("tes", result);
    }

    [Fact]
    public void EnableHashedShorteningWithNoTruncationNeededBehavesNormally()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 50,
            EnableHashedShortening = true
        });
        
        var result = helper.GenerateSlug("short");
        
        // Should not add hash if no truncation is needed
        Assert.Equal("short", result);
    }

    [Fact]
    public void HashedShorteningIsDisabledByDefault()
    {
        var config = new SlugHelperConfiguration();
        Assert.False(config.EnableHashedShortening);
    }

    [Fact]
    public void HashLengthDefaultsToTwo()
    {
        var config = new SlugHelperConfiguration();
        Assert.Equal(2, config.HashLength);
    }

    [Fact]
    public void EnableHashedShorteningWithCustomHashLength()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 15,
            EnableHashedShortening = true,
            HashLength = 4
        });
        
        var result = helper.GenerateSlug("The very long name that needs truncation");
        
        // Should be within the maximum length
        Assert.True(result.Length <= 15);
        
        // Should end with a dash followed by 4 hex characters
        Assert.Equal('-', result[result.Length - 5]);
        var hash = result.Substring(result.Length - 4);
        Assert.Equal(4, hash.Length);
        Assert.True(hash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
    }

    [Fact]
    public void EnableHashedShorteningWithHashLengthSix()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 20,
            EnableHashedShortening = true,
            HashLength = 6
        });
        
        var result = helper.GenerateSlug("The very long name that needs truncation");
        
        // Should be within the maximum length
        Assert.True(result.Length <= 20);
        
        // Should end with a dash followed by 6 hex characters
        Assert.Equal('-', result[result.Length - 7]);
        var hash = result.Substring(result.Length - 6);
        Assert.Equal(6, hash.Length);
        Assert.True(hash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
    }

    [Fact]
    public void EnableHashedShorteningHashLengthIsClamped()
    {
        // Test that hash length is clamped to valid range (2-6)
        var helperTooSmall = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 10,
            EnableHashedShortening = true,
            HashLength = 1 // Should be clamped to 2
        });
        
        var resultTooSmall = helperTooSmall.GenerateSlug("test input for hashing");
        var hashTooSmall = resultTooSmall.Substring(resultTooSmall.Length - 2);
        Assert.Equal(2, hashTooSmall.Length);
        
        var helperTooBig = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 15,
            EnableHashedShortening = true,
            HashLength = 10 // Should be clamped to 6
        });
        
        var resultTooBig = helperTooBig.GenerateSlug("test input for hashing");
        var hashTooBig = resultTooBig.Substring(resultTooBig.Length - 6);
        Assert.Equal(6, hashTooBig.Length);
    }

    [Fact]
    public void EnableHashedShorteningImprovedCollisionResistance()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 15,
            EnableHashedShortening = true,
            HashLength = 4 // Using 4 chars for better collision resistance
        });
        
        // Generate many different inputs with similar prefixes
        var inputs = new[]
        {
            "The very long name liga",
            "The very long name liga (W)",
            "The very long name liga (M)",
            "The very long name liga (L)",
            "The very long name liga (XL)",
            "The very long name liga (XXL)",
            "The very long name liga Black",
            "The very long name liga White",
            "The very long name liga Red",
            "The very long name liga Blue",
            "The very long name liga Green",
            "The very long name liga Yellow",
            "The very long name liga Orange",
            "The very long name liga Purple",
            "The very long name liga Pink",
            "The very long name liga Brown",
            "The very long name liga Gray",
            "The very long name liga Silver",
            "The very long name liga Gold",
            "The very long name liga Platinum"
        };
        
        var results = new HashSet<string>();
        var hashes = new HashSet<string>();
        
        foreach (var input in inputs)
        {
            var result = helper.GenerateSlug(input);
            var hash = result.Substring(result.Length - 4);
            
            // Each result should be unique
            Assert.True(results.Add(result), $"Duplicate result found: {result}");
            
            // Each hash should be unique (with 4 chars we have 65536 possibilities)
            Assert.True(hashes.Add(hash), $"Hash collision found: {hash} for input: {input}");
            
            // Verify result is within length limit
            Assert.True(result.Length <= 15);
            
            // Verify hash format
            Assert.True(hash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
        }
        
        // With 20 inputs and 4-char hash (65536 possibilities), we should have no collisions
        Assert.Equal(inputs.Length, results.Count);
        Assert.Equal(inputs.Length, hashes.Count);
    }

    [Fact]
    public void EnableHashedShorteningDeterministicAcrossPlatforms()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 12,
            EnableHashedShortening = true,
            HashLength = 4
        });
        
        // Test that the same input produces the same hash consistently
        var input = "The very long name that needs truncation";
        var result1 = helper.GenerateSlug(input);
        var result2 = helper.GenerateSlug(input);
        
        Assert.Equal(result1, result2);
        
        // Extract hash parts
        var hash1 = result1.Substring(result1.Length - 4);
        var hash2 = result2.Substring(result2.Length - 4);
        
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void EnableHashedShorteningWithForceLowerCaseFalse()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 12,
            EnableHashedShortening = true,
            ForceLowerCase = false
        });
        
        var result1 = helper.GenerateSlug("The Very Long Name Liga");
        var result2 = helper.GenerateSlug("The Very Long Name Liga (W)");
        
        // Should preserve case in the truncated part but still add hash
        Assert.True(result1.Length <= 12);
        Assert.True(result2.Length <= 12);
        Assert.NotEqual(result1, result2);
        
        // Both should end with hash pattern
        Assert.Equal('-', result1[result1.Length - 3]);
        Assert.Equal('-', result2[result2.Length - 3]);
        
        // Hash should be different
        var hash1 = result1.Substring(result1.Length - 2);
        var hash2 = result2.Substring(result2.Length - 2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void EnableHashedShorteningWithCustomStringReplacements()
    {
        var config = new SlugHelperConfiguration()
        {
            MaximumLength = 15,
            EnableHashedShortening = true
        };
        config.StringReplacements.Add("&", "and");
        config.StringReplacements.Add("@", "at");
        
        var helper = Create(config);
        
        var result1 = helper.GenerateSlug("Company & Partners @ Location");
        var result2 = helper.GenerateSlug("Company & Partners @ Different Location");
        
        // Should apply replacements before hashing and truncation
        Assert.True(result1.Length <= 15);
        Assert.True(result2.Length <= 15);
        Assert.NotEqual(result1, result2);
        
        // Should contain "and" and "at" in the processed slug
        var fullSlug1 = helper.GenerateSlug("Company & Partners @ Location");
        var fullSlug2 = helper.GenerateSlug("Company & Partners @ Different Location");
        
        // Hash should be calculated after replacements
        Assert.Equal('-', result1[result1.Length - 3]);
        Assert.Equal('-', result2[result2.Length - 3]);
    }

    [Fact]
    public void EnableHashedShorteningWithCollapseDashesFalse()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 12,
            EnableHashedShortening = true,
            CollapseDashes = false
        });
        
        var result = helper.GenerateSlug("word  &  another  word");
        
        // Should preserve multiple dashes but still truncate with hash
        Assert.True(result.Length <= 12);
        Assert.Equal('-', result[result.Length - 3]);
        
        // Last 2 characters should be valid hex
        var hash = result.Substring(result.Length - 2);
        Assert.True(hash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
    }

    [Fact]
    public void EnableHashedShorteningWithTrimWhitespaceFalse()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 15,
            EnableHashedShortening = true,
            TrimWhitespace = false
        });
        
        var result1 = helper.GenerateSlug("  long text with spaces  ");
        var result2 = helper.GenerateSlug("long text with spaces");
        
        // Should treat differently due to leading/trailing spaces
        Assert.True(result1.Length <= 15);
        Assert.True(result2.Length <= 15);
        Assert.NotEqual(result1, result2);
        
        // Both should have hash postfix
        Assert.Equal('-', result1[result1.Length - 3]);
        Assert.Equal('-', result2[result2.Length - 3]);
    }

    [Fact]
    public void EnableHashedShorteningWithDeniedCharactersRegex()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 12,
            EnableHashedShortening = true,
            DeniedCharactersRegex = new System.Text.RegularExpressions.Regex(@"[^a-zA-Z0-9\-]")
        });
        
        var result1 = helper.GenerateSlug("test.with_special@chars");
        var result2 = helper.GenerateSlug("test with special chars");
        
        // Should apply regex filtering before hashing
        Assert.True(result1.Length <= 12);
        Assert.True(result2.Length <= 12);
        Assert.NotEqual(result1, result2);
        
        // Should end with hash
        Assert.Equal('-', result1[result1.Length - 3]);
        Assert.Equal('-', result2[result2.Length - 3]);
    }

    [Fact]
    public void EnableHashedShorteningWithNonAsciiLanguages()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 15,
            EnableHashedShortening = true,
            SupportNonAsciiLanguages = true
        });
        
        var result1 = helper.GenerateSlug("很长的中文文本需要被截断");
        var result2 = helper.GenerateSlug("很长的中文文本需要被截断和处理");
        
        // Should handle non-ASCII characters and create unique hashes
        Assert.True(result1.Length <= 15);
        Assert.True(result2.Length <= 15);
        Assert.NotEqual(result1, result2);
        
        // Should end with hash
        Assert.Equal('-', result1[result1.Length - 3]);
        Assert.Equal('-', result2[result2.Length - 3]);
        
        // Hash should be different
        var hash1 = result1.Substring(result1.Length - 2);
        var hash2 = result2.Substring(result2.Length - 2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void EnableHashedShorteningWithModifiedAllowedCharacters()
    {
        var config = new SlugHelperConfiguration()
        {
            MaximumLength = 12,
            EnableHashedShortening = true
        };
        
        // Add some custom allowed characters
        config.AllowedCharacters.Add('!');
        config.AllowedCharacters.Add('*');
        // Remove some default ones
        config.AllowedCharacters.Remove('.');
        config.AllowedCharacters.Remove('_');
        
        var helper = Create(config);
        
        var result1 = helper.GenerateSlug("test.with_special!chars*");
        var result2 = helper.GenerateSlug("test with special!chars*");
        
        // Should respect modified allowed characters
        Assert.True(result1.Length <= 12);
        Assert.True(result2.Length <= 12);
        Assert.NotEqual(result1, result2);
        
        // Should end with hash
        Assert.Equal('-', result1[result1.Length - 3]);
        Assert.Equal('-', result2[result2.Length - 3]);
        
        // Should contain allowed special chars but not denied ones
        Assert.DoesNotContain(".", result1);
        Assert.DoesNotContain("_", result1);
    }

    [Fact]
    public void EnableHashedShorteningComplexCombination()
    {
        var config = new SlugHelperConfiguration()
        {
            MaximumLength = 20,
            EnableHashedShortening = true,
            ForceLowerCase = false,
            CollapseDashes = false,
            TrimWhitespace = true,
            SupportNonAsciiLanguages = true
        };
        
        config.StringReplacements.Add("&", " AND ");
        config.StringReplacements.Add("@", " AT ");
        
        var helper = Create(config);
        
        var result1 = helper.GenerateSlug("  Company & Associates @ München  ");
        var result2 = helper.GenerateSlug("  Company & Associates @ Berlin  ");
        
        // Should apply all transformations and create unique hashes
        Assert.True(result1.Length <= 20);
        Assert.True(result2.Length <= 20);
        Assert.NotEqual(result1, result2);
        
        // Should end with hash
        Assert.Equal('-', result1[result1.Length - 3]);
        Assert.Equal('-', result2[result2.Length - 3]);
        
        // Hash should be different
        var hash1 = result1.Substring(result1.Length - 2);
        var hash2 = result2.Substring(result2.Length - 2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void EnableHashedShorteningWithEdgeCaseInputs()
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 10,
            EnableHashedShortening = true
        });
        
        // Test with input that results in mostly dashes
        var result1 = helper.GenerateSlug("!@#$%^&*() test !@#$%^&*()");
        var result2 = helper.GenerateSlug("!@#$%^&*() different !@#$%^&*()");
        
        // Should handle edge cases gracefully
        Assert.True(result1.Length <= 10);
        Assert.True(result2.Length <= 10);
        Assert.NotEqual(result1, result2);
        
        // Only check hash pattern if result is at max length (meaning it was truncated)
        if (result1.Length == 10)
        {
            Assert.Equal('-', result1[result1.Length - 3]);
        }
        if (result2.Length == 10)
        {
            Assert.Equal('-', result2[result2.Length - 3]);
        }
    }

    [Theory]
    [InlineData("Test & Co. @ Location #1", "Test & Co. @ Location #2")]
    [InlineData("München Straße 123", "München Straße 456")]
    [InlineData("Company   &&&   Name", "Company   &&&   Different")]
    public void EnableHashedShorteningConsistentWithVariousInputs(string input1, string input2)
    {
        var helper = Create(new SlugHelperConfiguration()
        {
            MaximumLength = 15,
            EnableHashedShortening = true,
            SupportNonAsciiLanguages = true
        });
        
        var result1 = helper.GenerateSlug(input1);
        var result2 = helper.GenerateSlug(input2);
        
        // Should create different results for different inputs
        Assert.NotEqual(result1, result2);
        
        // Both should be within length limit
        Assert.True(result1.Length <= 15);
        Assert.True(result2.Length <= 15);
        
        // Both should have hash postfix if truncated
        if (result1.Length == 15)
        {
            Assert.Equal('-', result1[result1.Length - 3]);
            var hash1 = result1.Substring(result1.Length - 2);
            Assert.True(hash1.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
        }
        
        if (result2.Length == 15)
        {
            Assert.Equal('-', result2[result2.Length - 3]);
            var hash2 = result2.Substring(result2.Length - 2);
            Assert.True(hash2.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
        }
        
        // If both are at max length, hashes should be different
        if (result1.Length == 15 && result2.Length == 15)
        {
            var hash1 = result1.Substring(result1.Length - 2);
            var hash2 = result2.Substring(result2.Length - 2);
            Assert.NotEqual(hash1, hash2);
        }
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