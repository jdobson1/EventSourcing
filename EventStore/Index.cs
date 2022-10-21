using System;

namespace EventStore;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class Index : Attribute
{
    public Index(string property)
    {
        Property = property;
    }
    public string Property { get; set; }
}