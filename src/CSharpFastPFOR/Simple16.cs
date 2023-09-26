




/**
 * This is an implementation of the popular Simple16 scheme. It is limited to
 * 28-bit integers (between 0 and 2^28-1).
 * 
 * Note that this does not use differential coding: if you are working on sorted
 * lists, you must compute the deltas separately.
 * 
 * <p>
 * Adapted by D. Lemire from the Apache Lucene project.
 * </p>
 */

using System;

namespace Genbox.CSharpFastPFOR;

public class Simple16 : IntegerCODEC, SkippableIntegerCODEC
{

    public void headlessCompress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos)
    {
        int i_inpos = inpos.get();
        int i_outpos = outpos.get();
        int finalin = i_inpos + inlength;
        while (i_inpos < finalin)
        {
            int inoffset = compressblock(@out, i_outpos++, @in, i_inpos, inlength);
            if (inoffset == -1)
                throw new Exception("Too big a number");
            i_inpos += inoffset;
            inlength -= inoffset;
        }
        inpos.set(i_inpos);
        outpos.set(i_outpos);
    }

    /**
     * Compress an integer array using Simple16
     * 
     * @param out
     *            the compressed output
     * @param outOffset
     *            the offset of the output in the number of integers
     * @param in
     *            the integer input array
     * @param inOffset
     *            the offset of the input in the number of integers
     * @param n
     *            the number of elements to be compressed
     * @return the number of compressed integers
     */
    public static int compressblock(int[] @out, int outOffset, int[] @in, int inOffset, int n)
    {
        int numIdx, j, num, bits;
        for (numIdx = 0; numIdx < S16_NUMSIZE; numIdx++)
        {
            @out[outOffset] = numIdx << S16_BITSSIZE;
            num = (S16_NUM[numIdx] < n) ? S16_NUM[numIdx] : n;

            for (j = 0, bits = 0; (j < num)
                                  && (@in[inOffset + j] < SHIFTED_S16_BITS[numIdx][j]);)
            {
                @out[outOffset] |= (@in[inOffset + j] << bits);
                bits += S16_BITS[numIdx][j];
                j++;
            }

            if (j == num)
            {
                return num;
            }
        }

        return -1;
    }

    /**
     * Decompress an integer array using Simple16
     * 
     * @param out
     *            the decompressed output
     * @param outOffset
     *            the offset of the output in the number of integers
     * @param in
     *            the compressed input array
     * @param inOffset
     *            the offset of the input in the number of integers
     * @param n
     *            the number of elements to be compressed
     * @return the number of processed integers
     */
    public static int decompressblock(int[] @out, int outOffset, int[] @in, int inOffset, int n)
    {
        int numIdx, j = 0, bits = 0;
        numIdx = (int)((uint)@in[inOffset] >> S16_BITSSIZE);
        int num = S16_NUM[numIdx] < n ? S16_NUM[numIdx] : n;
        for (j = 0, bits = 0; j < num; j++)
        {
            @out[outOffset + j] = (int)((uint)@in[inOffset] >> bits) & (int)((uint)0xffffffff >> (32 - S16_BITS[numIdx][j]));
            bits += S16_BITS[numIdx][j];
        }
        return num;
    }

    public void headlessUncompress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos, int num)
    {
        int i_inpos = inpos.get();
        int i_outpos = outpos.get();
        while (num > 0)
        {
            int howmany = decompressblock(@out, i_outpos, @in, i_inpos, num);
            num -= howmany;
            i_outpos += howmany;
            i_inpos++;
        }
        inpos.set(i_inpos);
        outpos.set(i_outpos);
    }

    /**
     * Uncompress data from an array to another array.
     * 
     * Both inpos and outpos parameters are modified to indicate new positions
     * after read/write.
     * 
     * @param in
     *            array containing data in compressed form
     * @param tmpinpos
     *            where to start reading in the array
     * @param inlength
     *            length of the compressed data (ignored by some schemes)
     * @param out
     *            array where to write the compressed output
     * @param currentPos
     *            where to write the compressed output in out
     * @param outlength
     *            number of integers we want to decode
     */
    public static void uncompress(int[] @in, int tmpinpos, int inlength, int[] @out, int currentPos, int outlength)
    {
        int pos = tmpinpos + inlength;
        while (tmpinpos < pos)
        {
            int howmany = decompressblock(@out, currentPos, @in, tmpinpos,
               outlength);
            outlength -= howmany;
            currentPos += howmany;
            tmpinpos += 1;
        }
    }

    private static int[][] shiftme(int[][] x)
    {
        int[][] answer = new int[x.Length][];
        for (int k = 0; k < x.Length; ++k)
        {
            answer[k] = new int[x[k].Length];
            for (int z = 0; z < answer[k].Length; ++z)
                answer[k][z] = 1 << x[k][z];
        }
        return answer;
    }

    public void compress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos)
    {
        if (inlength == 0)
            return;
        @out[outpos.get()] = inlength;
        outpos.increment();
        headlessCompress(@in, inpos, inlength, @out, outpos);
    }

    public void uncompress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos)
    {
        if (inlength == 0)
            return;
        int outlength = @in[inpos.get()];
        inpos.increment();
        headlessUncompress(@in, inpos, inlength, @out, outpos, outlength);

    }

    private const int S16_NUMSIZE = 16;
    private const int S16_BITSSIZE = 28;
    // the possible number of bits used to represent one integer
    private static readonly int[] S16_NUM = { 28, 21, 21, 21, 14, 9, 8, 7, 6, 6, 5, 5, 4, 3, 2, 1 };
    // the corresponding number of elements for each value of the number of bits
    private static readonly int[][] S16_BITS = {
        new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,1, 1, 1, 1, 1, 1 },
        new[]{ 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        new[]{ 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1 },
        new[]{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2 },
        new[]{ 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
        new[]{ 4, 3, 3, 3, 3, 3, 3, 3, 3 }, new[] { 3, 4, 4, 4, 4, 3, 3, 3 },
        new[]{ 4, 4, 4, 4, 4, 4, 4 },  new[]{ 5, 5, 5, 5, 4, 4 },
        new[]{ 4, 4, 5, 5, 5, 5 }, new[] { 6, 6, 6, 5, 5 }, new[] { 5, 5, 6, 6, 6 },
        new[]{ 7, 7, 7, 7 }, new[] { 10, 9, 9, },  new[]{ 14, 14 },  new[]{ 28 } };
    private static readonly int[][] SHIFTED_S16_BITS = shiftme(S16_BITS);

    public override string ToString()
    {
        return nameof(Simple16);
    }
}