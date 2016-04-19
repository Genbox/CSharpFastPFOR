namespace CSharpFastPFOR
{
    class Program
    {
        static void Main(string[] args)
        {
            IntCompressor c = new IntCompressor(new SkippableComposition(new BinaryPacking(), new VariableByte()));

            int[] input = { 1, 2, 3, 4, 5, 6, 7 };
            int[] compressed = c.compress(input);

            int[] decompressed = c.uncompress(compressed);
        }
    }
}
