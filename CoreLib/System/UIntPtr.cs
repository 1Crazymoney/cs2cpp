////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Apache License 2.0 (Apache)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System;

    [Serializable]
    public struct UIntPtr
    {
        unsafe private void* _value;

        public static readonly UIntPtr Zero;

        public unsafe UIntPtr(void* value)
        {
            _value = value;
        }

        public unsafe UIntPtr(int value)
        {
            _value = (void*)value;
        }

        public unsafe void* ToPointer()
        {
            return _value;
        }
    }
}

