using System;
using UnityEngine;

namespace InGame.Extensions.Objects
{
    public static class ObjectsExtensions
    {
        public static bool IsNumericType(this Type o)
        {
            //Debug.Log("Is number " + type);
            //return (type.IsPrimitive && type != typeof(bool) && type != typeof(char));
            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
