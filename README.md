Slugify Core
=======

> This is a fork of the original project here: [https://github.com/fcingolani/Slugify](https://github.com/fcingolani/Slugify). This has been updated for .NET Standard 2.0 support (older versions support .NET Standard down to 1.3).

[![Build status](https://github.com/ctolkien/Slugify/actions/workflows/build.yml/badge.svg)](https://github.com/ctolkien/Slugify/actions/workflows/dotnet.yml)
[![Current NuGet release](https://img.shields.io/nuget/v/slugify.core.svg?maxAge=2000)](https://www.nuget.org/packages/Slugify.Core)
[![MIT license](https://img.shields.io/github/license/ctolkien/Slugify.svg?maxAge=2592000)](https://github.com/ctolkien/Slugify/blob/master/LICENSE)

Simple [Slug / Clean URL](http://en.wikipedia.org/wiki/Slug_%28web_publishing%29#Slug) generator helper for Microsoft .NET.

With default settings, you will get an **hyphenized**, **lowercase**, **alphanumeric** version of any string you please, with any [diacritics](http://en.wikipedia.org/wiki/Diacritic) removed, whitespace and dashes collapsed, and whitespace trimmed.

For example, having:

> a ambição cerra o coração

You'll get:

> a-ambicao-cerra-o-coracao

Installation
------------

You can get the [Slugify NuGet package](https://www.nuget.org/packages/Slugify.Core/) by running the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

```
PM> Install-Package Slugify.Core
```

Or running `dotnet add package Slugify.Core` from the command line.

Upgrading from 2.x to 3.x
-------------------------

* 3.0 is a significantly faster and less memory intensive version of the Slugifier. Whilst effort has been made to maintain backwards compatability, there may be some breaking changes.
* The `SlugHelper.Config` nested class has been renamed to just `SlugHelperConfiguration`.

Basic Usage
-----------

It's really simple! Just instantiate `SlugHelper` and call its `GenerateSlug` method with the **string** you want to convert; it'll return the slugified version:

```csharp
using Slugify;

public class MyApp
{
    public static void Main()
    {
        SlugHelper helper = new SlugHelper();

        String title = "OLA ke ase!";

        String slug = helper.GenerateSlug(title);

        Console.WriteLine(slug); // "ola-ke-ase"
    }
}
```

Configuration
-------------

The default configuration of `SlugHelper` will make the following changes to the passed input in order to generate a slug:

- Transform all characters to lower-case, to produce a lower-case slug.
- Trim all leading and trailing whitespace.
- Collapse all consecutive whitespace into a single space.
- Replace spaces with a dash.
- Remove all non-alphanumerical ASCII characters.
- Collapse all consecutive dashes into a single one.

You can customize most of this behavior by passing a `SlugHelperConfiguration` object to the `SlugHelper` constructor. For example, the following example will keep upper-case characters in the input and provides a custom handling for ampersands in the input:

```csharp
// Creating a configuration object
var config = new SlugHelperConfiguration();

// Add individual replacement rules
config.StringReplacements.Add("&", "-");
config.StringReplacements.Add(",", "-");

// Keep the casing of the input string
config.ForceLowerCase = false;

// Create a helper instance with our new configuration
var helper = new SlugHelper(config);

var result = helper.GenerateSlug("Simple,short&quick Example");
Console.WriteLine(result); // Simple-short-quick-Example
```

The following options can be configured with the `SlugHelperConfiguration`:

### `ForceLowerCase`
This specifies whether the output string should be converted to lower-case. If set to `false`, the original casing will be preserved. The lower-case conversion happens before any other character replacements are being made.

-  Default value: `true`

### `CollapseWhiteSpace`
This specifies whether consecutive whitespace should be replaced by just one space (`" "`). The whitespace will be collapsed before any other character replacements are being made.

- Default value: `true`

### `TrimWhitespace`
This specifies whether leading and trailing whitespace should be removed from the input string. The whitespace will be trimmed before any other character replacements are being made.

- Default value: `true`

### `CollapseDashes`
This specifies wehther consecutive dashes (`"-"`) should be collapsed into a single dash. This is useful to avoid scenarios like `"foo & bar"` becoming `"foo--bar"`. Dashes will be collapsed after all other string replacements have been made before the final result string is returned.

- Default value: `true`

### `StringReplacements`
This is a dictionary containing a mapping of characters that should be replaced individually before the translation happens. By default, this will replace space characters with a hyphen.

String replacements are being made after whitespace has been trimmed and collapsed, after the input string has been converted to lower-case characters, but before any characters are removed, to allow replacing characters that would otherwise be just removed.

-  Default value:

   ```csharp
   new Dictionary<string, string> {
      [" "] = "-", // replace space with a hyphen
   }
   ```

-  Examples:

   ```csharp
   var config = new SlugHelperConfiguration();

   // replace the dictionary completely
   config.StringReplacements = new() {
       ["ä"] = "ae",
       ["ö"] = "oe",
       ["ü"] = "ue",
   };

   // or add individual replacements to it
   config.StringReplacements.Add("ß", "ss");
   ```

### `AllowedChars`
Set of characters that are allowed in the slug, which will be kept when the input string is being processed. By default, this contains all ASCII characters, the full stop, the dash and the underscore. This is the preferred way of controlling which characters should be replaced when generating the slug.

Characters that are not allowed will be replaced after string replacements are completed.

-  Default value: Alphanumerical ASCII characters, the full stop (`.`), the dash (`-`), and the underscore (`-`).
   `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._`)

-  Examples:

   ```csharp
   var config = new SlugHelperConfiguration();

   // add individual characters to the list of allowed characters
   config.AllowedChars.Add('!');

   // remove previously added or default characters
   config.AllowedChars.Remove('.');
   ```

### `DeniedCharactersRegex`
Alternative method of specifying which characters will be allowed in the slug, which will replace the functionality of the `AllowedChars` set. The value must be a valid regular expression that specifies which characters *are to be removed*. Every match of this regular expression in the input string will be removed. The removal happens after string replacements are completed.

This functionality is kept in place for legacy compatibility reasons and since it relies on regular expressions, it will perform worse than using the `AllowedChars` way of specifying.

Specifying the `DeniedCharactersRegex` option will disable the character removal behavior from the `AllowedChars` option.

-  Default value: `null`

-  Examples:

   ```csharp
   var helper = new SlugHelper(new SlugHelperConfiguration
   {
       // this is equivalent to the default behavior from `AllowChars`
       DeniedCharactersRegex = "[^a-zA-Z0-9._-]"
   });
   Console.WriteLine(helper.GenerateSlug("OLA ke ase!")); // "ola-ke-ase"

   helper = new SlugHelper(new SlugHelperConfiguration
   {
       // remove certain characters explicitly
       DeniedCharactersRegex = @"[abcdef]"
   });
   Console.WriteLine(helper.GenerateSlug("abcdefghijk")); // "ghijk"

   helper = new SlugHelper(new SlugHelperConfiguration
   {
       // remove more complex matches
       DeniedCharactersRegex = @"foo|bar"
   });
   Console.WriteLine(helper.GenerateSlug("this is an foo example")); // "this-is-an-example"
   ```
