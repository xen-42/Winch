using System.Collections.Generic;
using Winch.Serialization;

namespace Winch.Data.WorldEvent.Condition;

public class NumConditionConverter : InventoryConditionConverter
{
    private readonly Dictionary<string, FieldDefinition> _definitions = new()
    {
        { "minNumber", new(1, o=> int.Parse(o.ToString())) }
    };

    public NumConditionConverter()
    {
        AddDefinitions(_definitions);
    }
}