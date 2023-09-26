/**
 * This code is released under the
 * Apache License Version 2.0 http://www.apache.org/licenses/.
 *
 * (c) Daniel Lemire, http://lemire.me/en/
 */

/**
 * Interface describing a CODEC to compress integers to bytes.
 * 
 * @author Daniel Lemire
 * 
 */
namespace Genbox.CSharpFastPFOR;

public interface ByteIntegerCODEC
{
    /**
     * Compress data from an array to another array.
     * 
     * Both inpos and outpos are modified to represent how much data was
     * read and written to if 12 ints (inlength = 12) are compressed to 3
     * bytes, then inpos will be incremented by 12 while outpos will be
     * incremented by 3 we use IntWrapper to pass the values by reference.
     * 
     * @param in
     *                input array
     * @param inpos
     *                location in the input array
     * @param inlength
     *                how many integers to compress
     * @param out
     *                output array
     * @param outpos
     *                where to write in the output array
     */
    void compress(int[] @in, IntWrapper inpos, int inlength, sbyte[] @out, IntWrapper outpos);

    /**
     * Uncompress data from an array to another array.
     * 
     * Both inpos and outpos parameters are modified to indicate new
     * positions after read/write.
     * 
     * @param in
     *                array containing data in compressed form
     * @param inpos
     *                where to start reading in the array
     * @param inlength
     *                length of the compressed data (ignored by some
     *                schemes)
     * @param out
     *                array where to write the compressed output
     * @param outpos
     *                where to write the compressed output in out
     */
    void uncompress(sbyte[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos);
}