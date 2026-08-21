using Unicode.NET;

var properties = new[] { "Lu", "BasicLatin", "Greek", "Alphabetic", "UnknownProp" };

foreach (var prop in properties)
{
    if (UnicodeProperties.TryResolve(prop, UnicodeVersion.Current, out var set))
    {
        Console.WriteLine($"{prop}: {set.Count} code points");
    }
    else
    {
        var suggestions = UnicodeProperties.Suggest(prop, UnicodeVersion.Current, maxSuggestions: 3);
        Console.WriteLine($"{prop}: Unknown. Did you mean: {string.Join(", ", suggestions)}?");
    }
}
