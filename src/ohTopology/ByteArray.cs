using System;
using System.Collections.Generic;

namespace OpenHome.Av
{
    internal class ByteArray
    {
        private static byte[] Reverse(byte[] aData, int aOffset, int aLength)
        {
            byte[] reverse = new byte[aLength];
            for (int i = 0; i < aLength; i++)
            {
                reverse[i] = aData[aOffset + aLength - 1 - i];
            }
            return reverse;
        }

        private static uint BigEndianToUint32(byte[] aValue, int aStartIndex)
        {
            if (System.BitConverter.IsLittleEndian)
            {
                byte[] value = Reverse(aValue, aStartIndex, 4);
                return System.BitConverter.ToUInt32(value, 0);
            }
            else
            {
                return System.BitConverter.ToUInt32(aValue, aStartIndex);
            }
        }

        internal static IList<uint> Unpack(byte[] aIdArray)
        {
            List<uint> ids = new List<uint>();
            // unpack id array into list of ids 
            uint j = 0;
            for (int i = 0; i < aIdArray.Length; i += 4, ++j)
            {
                uint value = BigEndianToUint32(aIdArray, i);
                ids.Add(value);
            }

            return ids;
        }
    }
}
