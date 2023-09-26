/**
 * This code is released under the
 * Apache License Version 2.0 http://www.apache.org/licenses/.
 *
 * (c) Daniel Lemire, http://lemire.me/en/
 */

/**
 * OptPFD  based on Simple16  by Yan et al.
 * <p>
 * Follows:
 * </p><p>
 * H. Yan, S. Ding, T. Suel, Inverted index compression and query processing
 * with optimized document ordering, in: WWW 09, 2009, pp. 401-410.
 * </p>
 * using Simple16 as the secondary coder.
 * 
 * It encodes integers in blocks of 128 integers. For arrays containing
 * an arbitrary number of integers, you should use it in conjunction
 * with another CODEC: 
 * 
 *  <pre>IntegerCODEC ic = new Composition(new OptPFDS16(), new VariableByte()).</pre>
 * 
 * Note that this does not use differential coding: if you are working on sorted
 * lists, you must compute the deltas separately.
 * 
 * For multi-threaded applications, each thread should use its own OptPFDS16
 * object.
 * 
 * @author Daniel Lemire
 */
namespace Genbox.CSharpFastPFOR;

public class OptPFDS16 : IntegerCODEC, SkippableIntegerCODEC
{
    private const int BLOCK_SIZE = 128;
    private readonly int[] exceptbuffer = new int[2 * BLOCK_SIZE];

    /**
     * Constructor for the OptPFDS16 CODEC.
     */
    public OptPFDS16()
    {
    }

    public void headlessCompress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos)
    {
        inlength = Util.greatestMultiple(inlength, BLOCK_SIZE);
        if (inlength == 0)
            return;

        encodePage(@in, inpos, inlength, @out, outpos);
    }

    private static readonly int[] bits = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        11, 12, 13, 16, 20, 32 };
    private static readonly int[] invbits = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
        10, 11, 12, 13, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 16, 16,
        16, 16, 16, 16, 16, 16, 16 };

    private void getBestBFromData(int[] @in, int pos, IntWrapper bestb, IntWrapper bestexcept)
    {
        int mb = Util.maxbits(@in, pos, BLOCK_SIZE);
        int mini = 0;
        if (mini + 28 < bits[invbits[mb]])
            mini = bits[invbits[mb]] - 28; // 28 is the max for
        // exceptions
        int besti = bits.Length - 1;
        int bestcost = bits[besti] * 4;
        int exceptcounter = 0;
        for (int i = mini; i < bits.Length - 1; ++i)
        {
            int tmpcounter = 0;
            for (int k = pos; k < BLOCK_SIZE + pos; ++k)
                if ((int)((uint)@in[k] >> bits[i]) != 0)
                {
                    ++tmpcounter;
                }
            if (tmpcounter == BLOCK_SIZE)
                continue; // no need
            for (int k = pos, c = 0; k < pos + BLOCK_SIZE; ++k)
                if ((int)((uint)@in[k] >> bits[i]) != 0)
                {
                    exceptbuffer[tmpcounter + c] = k - pos;
                    exceptbuffer[c] = (int)((uint)@in[k] >> bits[i]);
                    ++c;
                }

            int thiscost = bits[i] * 4 + S16.estimatecompress(exceptbuffer, 0, 2 * tmpcounter);
            if (thiscost <= bestcost)
            {
                bestcost = thiscost;
                besti = i;
                exceptcounter = tmpcounter;
            }
        }
        bestb.set(besti);
        bestexcept.set(exceptcounter);
    }

    private void encodePage(int[] @in, IntWrapper inpos, int thissize, int[] @out, IntWrapper outpos)
    {
        int tmpoutpos = outpos.get();
        int tmpinpos = inpos.get();
        var bestb = new IntWrapper();
        var bestexcept = new IntWrapper();
        for (int finalinpos = tmpinpos + thissize; tmpinpos + BLOCK_SIZE <= finalinpos; tmpinpos += BLOCK_SIZE)
        {
            getBestBFromData(@in, tmpinpos, bestb, bestexcept);
            int tmpbestb = bestb.get();
            int nbrexcept = bestexcept.get();
            int exceptsize = 0;
            int remember = tmpoutpos;
            tmpoutpos++;
            if (nbrexcept > 0)
            {
                int c = 0;
                for (int i = 0; i < BLOCK_SIZE; ++i)
                {
                    if ((int)((uint)@in[tmpinpos + i] >> bits[tmpbestb]) != 0)
                    {
                        exceptbuffer[c + nbrexcept] = i;
                        exceptbuffer[c] = (int)((uint)@in[tmpinpos + i] >> bits[tmpbestb]);
                        ++c;
                    }
                }
                exceptsize = S16.compress(exceptbuffer, 0,
                    2 * nbrexcept, @out, tmpoutpos);
                tmpoutpos += exceptsize;
            }
            @out[remember] = tmpbestb | (nbrexcept << 8)
                             | (exceptsize << 16);
            for (int k = 0; k < BLOCK_SIZE; k += 32)
            {
                BitPacking.fastpack(@in, tmpinpos + k, @out,
                    tmpoutpos, bits[tmpbestb]);
                tmpoutpos += bits[tmpbestb];
            }
        }
        inpos.set(tmpinpos);
        outpos.set(tmpoutpos);
    }

    public void headlessUncompress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos, int mynvalue)
    {
        if (inlength == 0)
            return;
        mynvalue = Util.greatestMultiple(mynvalue, BLOCK_SIZE);
        decodePage(@in, inpos, @out, outpos, mynvalue);
    }

    private void decodePage(int[] @in, IntWrapper inpos, int[] @out, IntWrapper outpos, int thissize)
    {
        int tmpoutpos = outpos.get();
        int tmpinpos = inpos.get();

        for (int run = 0; run < thissize / BLOCK_SIZE; ++run, tmpoutpos += BLOCK_SIZE)
        {
            int b = @in[tmpinpos] & 0xFF;
            int cexcept = (int)((uint)@in[tmpinpos] >> 8) & 0xFF;
            int exceptsize = (int)((uint)@in[tmpinpos] >> 16);
            ++tmpinpos;
            S16.uncompress(@in, tmpinpos, exceptsize, exceptbuffer,
                0, 2 * cexcept);
            tmpinpos += exceptsize;
            for (int k = 0; k < BLOCK_SIZE; k += 32)
            {
                BitPacking.fastunpack(@in, tmpinpos, @out,
                    tmpoutpos + k, bits[b]);
                tmpinpos += bits[b];
            }
            for (int k = 0; k < cexcept; ++k)
            {
                @out[tmpoutpos + exceptbuffer[k + cexcept]] |= (exceptbuffer[k] << bits[b]);
            }
        }
        outpos.set(tmpoutpos);
        inpos.set(tmpinpos);
    }

    public void compress(int[] @in, IntWrapper inpos, int inlength, int[] @out, IntWrapper outpos)
    {
        inlength = inlength / BLOCK_SIZE * BLOCK_SIZE;
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

    public override string ToString()
    {
        return nameof(OptPFDS16);
    }
}