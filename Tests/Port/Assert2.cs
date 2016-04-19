using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpFastPFOR.Tests.Port
{
    public static class Assert2
    {
        public static void assertArrayEquals(int[] ints, int[] ints1)
        {
            Assert.IsTrue(ints.SequenceEqual(ints1));
        }

        public static void assertEquals(int first, int second)
        {
            Assert.AreEqual(first,second);
        }

        public static void assertTrue(bool b)
        {
            Assert.IsTrue(b);
        }
    }
}
