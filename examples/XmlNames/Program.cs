using Unicode.NET;
using Unicode.NET.Xml;

var testNames = new[] { "validName", "123invalid", "valid_name", "valid-name", "invalid name" };

foreach (var name in testNames)
{
    var isValid = IsValidXmlName(name);
    Console.WriteLine($"{name}: {(isValid ? "VALID" : "INVALID")}");
}

static bool IsValidXmlName(string name)
{
    if (string.IsNullOrEmpty(name)) return false;

    var firstChar = CodePoint.Create(name[0]);
    if (!XmlCharacterSets.NameStartChar.Contains(firstChar)) return false;

    foreach (var ch in name.Skip(1))
    {
        if (!XmlCharacterSets.NameChar.Contains(CodePoint.Create(ch))) return false;
    }

    return true;
}
