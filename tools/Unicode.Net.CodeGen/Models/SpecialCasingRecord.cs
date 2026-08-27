namespace Unicode.NET.CodeGen.Models;

public enum SpecialCasingConditionKind
{
  LocaleTag,
  ContextPredicate,
}

public sealed class SpecialCasingCondition
{
  public required string Token { get; init; }
  public required SpecialCasingConditionKind Kind { get; init; }
}

/// <summary>Represents one record from SpecialCasing.txt.</summary>
public sealed class SpecialCasingRecord
{
  public required int Source { get; init; }
  public required int[] LowercaseMapping { get; init; }
  public required int[] TitlecaseMapping { get; init; }
  public required int[] UppercaseMapping { get; init; }
  public required IReadOnlyList<SpecialCasingCondition> Conditions { get; init; }

  public int CodePoint => Source;
  public IReadOnlyList<string> LocaleTags =>
    Conditions.Where(c => c.Kind == SpecialCasingConditionKind.LocaleTag).Select(c => c.Token).ToArray();
  public IReadOnlyList<string> ContextPredicates =>
    Conditions.Where(c => c.Kind == SpecialCasingConditionKind.ContextPredicate).Select(c => c.Token).ToArray();
}
