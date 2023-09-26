



/**
 * This is a convenience class that wraps a codec to provide
 * a "friendly" API.
 *
 */

using Genbox.CSharpFastPFOR.Port;

namespace Genbox.CSharpFastPFOR;

public class IntCompressor
{
    private SkippableIntegerCODEC codec;

    /**
     * Constructor wrapping a codec.
     * 
     * @param c the underlying codec
     */
    public IntCompressor(SkippableIntegerCODEC c)
    {
        codec = c;
    }

    /**
     * Constructor with default codec.
     */
    public IntCompressor()
    {
        codec = new SkippableComposition(new BinaryPacking(), new VariableByte());
    }

    /**
     * Compress an array and returns the compressed result as a new array.
     * 
     * @param input array to be compressed
     * @return compressed array
     */
    public int[] compress(int[] input)
    {
        int[] compressed = new int[input.Length + 1024];
        compressed[0] = input.Length;
        IntWrapper outpos = new IntWrapper(1);
        codec.headlessCompress(input, new IntWrapper(0), input.Length, compressed, outpos);
        compressed = Arrays.copyOf(compressed, outpos.intValue());
        return compressed;
    }

    /**
     * Uncompress an array and returns the uncompressed result as a new array.
     * 
     * @param compressed compressed array
     * @return uncompressed array
     */
    public int[] uncompress(int[] compressed)
    {
        int[] decompressed = new int[compressed[0]];
        IntWrapper inpos = new IntWrapper(1);
        codec.headlessUncompress(compressed, inpos, compressed.Length - inpos.intValue(), decompressed, new IntWrapper(0), decompressed.Length);
        return decompressed;
    }
}