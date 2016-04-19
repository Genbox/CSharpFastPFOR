/*
 * This code is released under the
 * Apache License Version 2.0 http://www.apache.org/licenses/.
 */

/**
 * Delta+Zigzag Encoding.
 * 
 * @author MURAOKA Taro http://github.com/koron
 */
namespace CSharpFastPFOR
{
    public /* final */ class DeltaZigzagEncoding
    {
        public class Context
        {
            protected int ContextValue;

            public Context(int contextValue)
            {
                this.ContextValue = contextValue;
            }

            public void setContextValue(int contextValue)
            {
                this.ContextValue = contextValue;
            }

            public int getContextValue()
            {
                return this.ContextValue;
            }
        }

        public class Encoder : Context
        {
            public Encoder(int contextValue) : base(contextValue)
            {
            }

            public int encodeInt(int value)
            {
                int n = value - this.ContextValue;
                this.ContextValue = value;
                return (n << 1) ^ (n >> 31);
            }

            public int[] encodeArray(int[] src, int srcoff, int length,
                int[] dst, int dstoff)
            {
                for (int i = 0; i < length; ++i)
                {
                    dst[dstoff + i] = encodeInt(src[srcoff + i]);
                }
                return dst;
            }

            public int[] encodeArray(int[] src, int srcoff, int length,
                int[] dst)
            {
                return encodeArray(src, srcoff, length, dst, 0);
            }

            public int[] encodeArray(int[] src, int offset, int length)
            {
                return encodeArray(src, offset, length,
                    new int[length], 0);
            }

            public int[] encodeArray(int[] src)
            {
                return encodeArray(src, 0, src.Length,
                    new int[src.Length], 0);
            }
        }

        public class Decoder : Context
        {
            public Decoder(int contextValue) : base(contextValue)
            {
            }

            public int decodeInt(int value)
            {
                int n = (int)((uint)value >> 1) ^ ((value << 31) >> 31);
                n += this.ContextValue;
                this.ContextValue = n;
                return n;
            }

            public int[] decodeArray(int[] src, int srcoff, int length,
                int[] dst, int dstoff)
            {
                for (int i = 0; i < length; ++i)
                {
                    dst[dstoff + i] = decodeInt(src[srcoff + i]);
                }
                return dst;
            }

            public int[] decodeArray(int[] src, int offset, int length)
            {
                return decodeArray(src, offset, length,
                    new int[length], 0);
            }

            public int[] decodeArray(int[] src)
            {
                return decodeArray(src, 0, src.Length);
            }
        }
    }
}
