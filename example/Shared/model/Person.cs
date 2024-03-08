namespace Model;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Person))]
public partial class PersonSourceGenerationContext : JsonSerializerContext
{
}

public class Person
{
    public string Name { get; set; }

    public Person(string name)
    {
        Name = name;
    }
}
