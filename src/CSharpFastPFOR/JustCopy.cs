/**
 * This code is released under the
 * Apache License Version 2.0 http://www.apache.org/licenses/.
 *
 * (c) Daniel Lemire, http://lemire.me/en/
 */

/**
 * @author Daniel Lemire
 * 
 */

using System;

namespace Genbox.CSharpFastPFOR;

public class JustCopy : IntegerCODEC, SkippableIntegerCODEC
{
    public void headlessCompress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos)
    {
        Array.Copy(@in, inpos.get(), @out, outpos.get(), inlength);
        inpos.add(inlength);
        outpos.add(inlength);
    }

    public void uncompress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos)
    {
        headlessUncompress(@in, inpos, inlength, @out, outpos, inlength);
    }

    public void headlessUncompress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos, int num)
    {
        Array.Copy(@in, inpos.get(), @out, outpos.get(), num);
        inpos.add(num);
        outpos.add(num);
    }

    public void compress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos)
    {
        headlessCompress(@in, inpos, inlength, @out, outpos);
    }

    public override string ToString()
    {
        return nameof(JustCopy);
    }
}