////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Apache License 2.0 (Apache)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System;
    public interface ICustomFormatter
    {
        // Interface does not need to be marked with the serializable attribute
        String Format(String format, Object arg, IFormatProvider formatProvider);

    }
}


