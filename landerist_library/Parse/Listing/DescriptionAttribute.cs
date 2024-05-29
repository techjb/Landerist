namespace landerist_library.Parse.Listing
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class DescriptionAttribute(string description) : Attribute
    {
        public string Description { get; } = description;
    }
}
