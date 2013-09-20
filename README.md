Slugify
=======

URL Slug generator helper for Microsoft .NET framework

Basic Usage
-----------


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

You can provide a _SlugHelper.Config_ instance to _SlugHelper_'s constructor to customize the helper's behavior, like so:

```csharp
SlugHelper.Config config = new SlugHelper.Config();
SlugHelper helper = new SlugHelper(config);
```

### Options

#### CharacterReplacements

Type: _Dictionary&lt;String, String&gt;_. Default: **Empty**.

#### ForceLowerCase

Type: _Boolean_. Default: **true**.

Setting it to true will convert output string to be LowerCase. If false, original casing will be preserved.

#### CollapseWhiteSpace

Type: _Boolean_. Default: **true**.

Setting it to true will replace consecutive whitespace characters by just one space (" ").

#### DeniedCharactersRegex

Type: _String_. Default: **[^a-zA-Z0-9\-\._]**.

Any character matching this Regular Expression will be deleted from the resulting string.

