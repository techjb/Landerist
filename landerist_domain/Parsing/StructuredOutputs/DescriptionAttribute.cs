namespace landerist_domain.Parsing.StructuredOutputs
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class DescriptionAttribute(string description) : Attribute
    {
        public string Description { get; } = description;
    }
}
