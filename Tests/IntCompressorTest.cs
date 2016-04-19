
/**
 * Testing IntCompressor objects.
 */

using System;
using CSharpFastPFOR.Differential;
using CSharpFastPFOR.Port;
using CSharpFastPFOR.Tests.Port;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpFastPFOR.Tests
{
    [TestClass]
    public class IntCompressorTest
    {
        IntegratedIntCompressor[] iic = {
            new IntegratedIntCompressor(new IntegratedVariableByte()),
            new IntegratedIntCompressor(
                new SkippableIntegratedComposition(
                    new IntegratedBinaryPacking(),
                    new IntegratedVariableByte())) };
        IntCompressor[] ic = {
            new IntCompressor(new VariableByte()),
            new IntCompressor(new SkippableComposition(new BinaryPacking(),
                new VariableByte())) };

        [TestMethod]
        public void basicTest()
        {
            for (int N = 1; N <= 10000; N *= 10)
            {
                int[] orig = new int[N];
                for (int k = 0; k < N; k++)
                    orig[k] = 3 * k + 5;
                foreach (IntCompressor i in ic)
                {
                    int[] comp = i.compress(orig);
                    int[] back = i.uncompress(comp);
                    Assert2.assertArrayEquals(back, orig);
                }
            }
        }

        [TestMethod]
        public void superSimpleExample()
        {
            IntegratedIntCompressor iic = new IntegratedIntCompressor();
            int[] data = new int[2342351];
            for (int k = 0; k < data.Length; ++k)
                data[k] = k;
            Console.WriteLine("Compressing " + data.Length + " integers using friendly interface");
            int[] compressed = iic.compress(data);
            int[] recov = iic.uncompress(compressed);
            Console.WriteLine("compressed from " + data.Length * 4 / 1024 + "KB to " + compressed.Length * 4 / 1024 + "KB");
            if (!Arrays.equals(recov, data)) throw new Exception("bug");
        }

        [TestMethod]
        public void basicIntegratedTest()
        {
            for (int N = 1; N <= 10000; N *= 10)
            {
                int[] orig = new int[N];
                for (int k = 0; k < N; k++)
                    orig[k] = 3 * k + 5;
                foreach (IntegratedIntCompressor i in iic)
                {
                    int[] comp = i.compress(orig);
                    int[] back = i.uncompress(comp);
                    Assert2.assertArrayEquals(back, orig);
                }
            }
        }
    }
}