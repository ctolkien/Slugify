Slugify Core
=======

### This is a fork of the original project here: [https://github.com/fcingolani/Slugify](https://github.com/fcingolani/Slugify). This has been updated for .Net Core Support

| Platform | Status|
|---------|-------|
|Windows  | [![Build status](https://img.shields.io/appveyor/ci/soda-digital/slugify.svg?maxAge=2000)](https://ci.appveyor.com/project/Soda-Digital/slugify) |
|Linux/OSX| [![Build Status](https://img.shields.io/travis/ctolkien/Slugify.svg?maxAge=2000)](https://travis-ci.org/ctolkien/Slugify) |

Simple [Slug / Clean URL](http://en.wikipedia.org/wiki/Slug_%28web_publishing%29#Slug) generator helper for Microsoft .NET framework.

With default settings, you will get an **hyphenized**, **lowercase**, **alphanumeric** version of any string you please, with any [diacritics](http://en.wikipedia.org/wiki/Diacritic) removed and collapsed whitespace, collapsed dashes and trimmed whitespace.

In example, having:

> a ambição cerra o coração

You'll get:

> a-ambicao-cerra-o-coracao

Installation
------------

You can get the [Slugify NuGet package](http://www.nuget.org/packages/SlugifyCore/) by running the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

```
PM> Install-Package SlugifyCore
```


Basic Usage
-----------

It's really simple! Just instantiate _SlugHelper_ and call its _GenerateSlug_ with the **string** you want to convert; it'll return the URL Safe version:


```csharp
using Slugify;

public class MyApp
{
   public static void Main()
   {
      SlugHelper helper = new SlugHelper();

      String title = "OLA ke ase!";

      String slug = helper.GenerateSlug(title); // "ola-ke-ase"

      Console.WriteLine(slug);
   }
}

```

Configuration
-------------

You can provide a _SlugHelper.Config_ instance to _SlugHelper_'s constructor to customize the helper's behavior:

```csharp
// Creating a configuration object
SlugHelper.Config config = new SlugHelper.Config();

// Replace spaces with a dash
config.StringReplacements.Add(" ", "-");

// We want a lowercase Slug
config.ForceLowerCase = true;

// Will collapse multiple seqential dashes down to a single one
config.CollapseDashes = true;

// Will trim leading and trailing whitespace
config.TrimWhitespace = true;

// Colapse consecutive whitespace chars into one
config.CollapseWhiteSpace = true;

// Remove everything that's not a letter, number, hyphen, dot, or underscore
config.DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._]";

// Create a helper instance with our new configuration
SlugHelper helper = new SlugHelper(config);
```

In fact, last values are so common they're the default ones! So last code could be rewritten as:

```csharp
SlugHelper.Config config = new SlugHelper.Config();
SlugHelper helper = new SlugHelper(config);
```

One more thing: _SlugHelper.Config_ is used when you call the parameterless _SlugHelper_ constructor. Then ...

```csharp
SlugHelper helper = new SlugHelper();
```

... is the same as running the code we had in first place.

### Options

#### CharacterReplacements

Type: _Dictionary&lt;String, String&gt;_. Default: [" ": "-"].

Will replace the specified keys with their associated value.

By default, will replace spaces with hyphens.

#### ForceLowerCase

Type: _Boolean_. Default: **true**.

Setting it to true will convert output string to be LowerCase. If false, original casing will be preserved.

#### CollapseWhiteSpace

Type: _Boolean_. Default: **true**.

Setting it to true will replace consecutive whitespace characters by just one space (" ").

#### DeniedCharactersRegex

Type: _String_. Default: **[^a-zA-Z0-9\-\._]**.

Any character matching this Regular Expression will be deleted from the resulting string.

License
-------

The MIT License (MIT)

Copyright (c) 2013 Federico Cingolani

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
