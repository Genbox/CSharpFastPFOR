using System;
using System.IO;

namespace Genbox.CSharpFastPFOR.Port;

public class IntBuffer
{
    private readonly MemoryStream _ms;
    private readonly ByteOrder _order;

    public IntBuffer(MemoryStream ms, ByteOrder order)
    {
        _ms = ms;
        _order = order;
    }

    public void get(int[] dst, int offset, int length)
    {
        int end = offset + length;

        BinaryReader br = new BinaryReader(_ms);

        for (int i = offset; i < end; i++)
        {
            int value = br.ReadInt32();

            if (BitConverter.IsLittleEndian && _order == ByteOrder.BIG_ENDIAN)
                value = reverseBytes(value);

            dst[i] = value;
        }
    }

    public static int reverseBytes(int i)
    {
        return ((int)((uint)i >> 24)) |
               ((i >> 8) & 0xFF00) |
               ((i << 8) & 0xFF0000) |
               ((i << 24));
    }

    private static void checkBounds(int off, int len, int size)
    {
        if ((off | len | (off + len) | (size - (off + len))) < 0)
            throw new IndexOutOfRangeException();
    }
}