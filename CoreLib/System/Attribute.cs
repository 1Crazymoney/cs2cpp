////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Apache License 2.0 (Apache)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System;
    using System.Reflection;
    using System.Collections;

    [Serializable, AttributeUsageAttribute(AttributeTargets.All, Inherited = true, AllowMultiple = false)] // Base class for all attributes
    public abstract class Attribute
    {
        protected Attribute() { }
    }
}


