namespace Slugify;

/// <summary>
/// Provides configuration settings for SlugHelper and generates a URL-friendly slug from a given string. 
/// The slug is created by normalizing and replacing characters.
/// </summary>
public interface ISlugHelper
{
    /// <summary>
    /// Holds the configuration settings for the SlugHelper. It can be accessed and modified through its getter and
    /// setter.
    /// </summary>
    SlugHelperConfiguration Config { get; set; }

    /// <summary>
    /// Generates a URL-friendly slug from the provided string by normalizing and replacing characters.
    /// </summary>
    /// <param name="inputString">The string to be transformed into a slug format.</param>
    /// <returns>A string that represents the slug version of the input, with specified transformations applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    string GenerateSlug(string inputString);
}