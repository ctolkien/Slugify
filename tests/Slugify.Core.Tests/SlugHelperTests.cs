﻿using System;
using System.Globalization;
using Xunit;

namespace Slugify.Tests;

public class SlugHelperTest
{
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static ISlugHelper Create() => Create(new SlugHelperConfiguration());
    private static ISlugHelper CreateNonAscii() => CreateNonAscii(new SlugHelperConfiguration());
    private static ISlugHelper Create(SlugHelperConfiguration config) => new SlugHelper(config);
    private static ISlugHelper CreateNonAscii(SlugHelperConfiguration config) => new SlugHelperForNonAsciiLanguages(config);
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

    [Fact]
    public void TestEmptyConfig()
    {
        var config = new SlugHelperConfiguration();
        Assert.True(config.ForceLowerCase);
        Assert.True(config.CollapseWhiteSpace);
        Assert.Single(config.StringReplacements);
        Assert.Null(config.DeniedCharactersRegex);
        Assert.NotEmpty(config.AllowedChars);
    }

    [Fact]
    public void TestDeniedCharacterConfig()
    {
        var config = new SlugHelperConfiguration
        {
            DeniedCharactersRegex = new System.Text.RegularExpressions.Regex(string.Empty)
        };

        Assert.Throws<InvalidOperationException>(() => config.AllowedChars);
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
    public void TestWhiteSpaceNotCollapsing(ISlugHelper helper)
    {
        const string original = "a  b    \n  c   \t    d";
        const string expected = "a--b-------c--------d";

        helper.Config.CollapseDashes = false; 
        helper.Config.CollapseWhiteSpace = false;

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
        config.AllowedChars.Remove('a');
        config.AllowedChars.Add('$');
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
    public void TestDeniedCharacterDeletionLegacyKeepsAllowedCharacters(ISlugHelper helper)
    {
        const string original = "Abc-123.$1$_x";
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


#pragma warning disable xUnit1004 // Test methods should not be skipped
    [Fact(Skip = "We are not culture aware and do not support this.")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
    public void TurkishEncodingOfI()
    {
        //Set culture to Turkish
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        const string original = "FIFA 18";
        const string expected = "fıfa 18";

        var helper = CreateNonAscii();

        Assert.Equal(expected, helper.GenerateSlug(original));

    }

    [Fact]
    public void Arabic()
    {
        const string original = "نوشتار فارسی";
        const string expected = "nwshtr-frsy";

        var helper = CreateNonAscii();

        Assert.Equal(expected, helper.GenerateSlug(original));
    }

    [Fact]
    public void Cyrillic()
    {
        const string original = "Наизусть";
        const string expected = "naizust";

        var helper = CreateNonAscii();

        Assert.Equal(expected, helper.GenerateSlug(original));
    }
}