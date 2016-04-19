
/**
 * Just some basic sanity tests.
 * 
 * @author Daniel Lemire
 */

using System;
using CSharpFastPFOR.Differential;
using CSharpFastPFOR.Port;
using CSharpFastPFOR.Tests.Port;
using CSharpFastPFOR.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpFastPFOR.Tests
{
    [TestClass]
    public class ByteBasicTest
    {
        ByteIntegerCODEC[] codecs = {
            new VariableByte(),
            new IntegratedVariableByte(),
        };

        [TestMethod]
        public void saulTest()
        {
            foreach (ByteIntegerCODEC C in codecs)
            {
                for (int x = 0; x < 50 * 4; ++x)
                {
                    int[] a = { 2, 3, 4, 5 };
                    sbyte[] b = new sbyte[90 * 4];
                    int[] c = new int[a.Length];

                    IntWrapper aOffset = new IntWrapper(0);
                    IntWrapper bOffset = new IntWrapper(x);
                    C.compress(a, aOffset, a.Length, b, bOffset);
                    int len = bOffset.get() - x;

                    bOffset.set(x);
                    IntWrapper cOffset = new IntWrapper(0);
                    C.uncompress(b, bOffset, len, c, cOffset);
                    if (!Arrays.equals(a, c))
                    {
                        Console.WriteLine("Problem with " + C);
                    }
                    Assert2.assertArrayEquals(a, c);
                }
            }
        }

        [TestMethod]
        public void varyingLengthTest()
        {
            int N = 4096;
            int[] data = new int[N];
            for (int k = 0; k < N; ++k)
                data[k] = k;
            foreach (ByteIntegerCODEC c in codecs)
            {
                for (int L = 1; L <= 128; L++)
                {
                    sbyte[] comp = TestUtils.compress(c, Arrays.copyOf(data, L));
                    int[] answer = TestUtils.uncompress(c, comp, L);
                    for (int k = 0; k < L; ++k)
                        if (answer[k] != data[k])
                            throw new Exception("bug " + c + " " + k + " " + answer[k] + " " + data[k]);
                }
                for (int L = 128; L <= N; L *= 2)
                {
                    sbyte[] comp = TestUtils.compress(c, Arrays.copyOf(data, L));
                    int[] answer = TestUtils.uncompress(c, comp, L);
                    for (int k = 0; k < L; ++k)
                        if (answer[k] != data[k])
                            throw new Exception("bug");
                }
            }
        }

        [TestMethod]
        public void varyingLengthTest2()
        {
            int N = 128;
            int[] data = new int[N];
            data[127] = -1;
            foreach (ByteIntegerCODEC c in codecs)
            {
                //TODO: this makes no sense in port
                //if (c is Simple9)
                //    continue;

                for (int L = 1; L <= 128; L++)
                {
                    sbyte[] comp = TestUtils.compress(c, Arrays.copyOf(data, L));
                    int[] answer = TestUtils.uncompress(c, comp, L);
                    for (int k = 0; k < L; ++k)
                        if (answer[k] != data[k])
                            throw new Exception("bug at k = " + k + " " + answer[k] + " " + data[k]);
                }
                for (int L = 128; L <= N; L *= 2)
                {
                    sbyte[] comp = TestUtils.compress(c, Arrays.copyOf(data, L));
                    int[] answer = TestUtils.uncompress(c, comp, L);
                    for (int k = 0; k < L; ++k)
                        if (answer[k] != data[k])
                            throw new Exception("bug");
                }
            }
        }
    }
}