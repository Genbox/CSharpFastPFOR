/*
 * This code is released under the
 * Apache License Version 2.0 http://www.apache.org/licenses/.
 */

/**
 * BinaryPacking with Delta+Zigzag Encoding.
 * 
 * It encodes integers in blocks of 128 integers. For arrays containing
 * an arbitrary number of integers, you should use it in conjunction
 * with another CODEC: 
 * 
 *  <pre>IntegerCODEC ic = new Composition(new DeltaZigzagBinaryPacking(),
 *                      new DeltaZigzagVariableByte()).</pre>
 * 
 * @author MURAOKA Taro http://github.com/koron
 */

namespace Genbox.CSharpFastPFOR;

public class DeltaZigzagBinaryPacking : IntegerCODEC
{
    private const int BLOCK_LENGTH = 128;

    public void compress(int[] inBuf, IntWrapper inPos, int inLen, int[] outBuf, IntWrapper outPos)
    {
        inLen = inLen - inLen % BLOCK_LENGTH;
        if (inLen == 0)
        {
            return;
        }

        outBuf[outPos.get()] = inLen;
        outPos.increment();

        var ctx = new DeltaZigzagEncoding.Encoder(0);
        int[] work = new int[BLOCK_LENGTH];

        int op = outPos.get();
        int ip = inPos.get();
        int inPosLast = ip + inLen;
        for (; ip < inPosLast; ip += BLOCK_LENGTH)
        {
            ctx.encodeArray(inBuf, ip, BLOCK_LENGTH, work);
            int bits1 = Util.maxbits32(work, 0);
            int bits2 = Util.maxbits32(work, 32);
            int bits3 = Util.maxbits32(work, 64);
            int bits4 = Util.maxbits32(work, 96);
            outBuf[op++] = (bits1 << 24) | (bits2 << 16)
                           | (bits3 << 8) | (bits4 << 0);
            op += pack(work, 0, outBuf, op, bits1);
            op += pack(work, 32, outBuf, op, bits2);
            op += pack(work, 64, outBuf, op, bits3);
            op += pack(work, 96, outBuf, op, bits4);
        }

        inPos.add(inLen);
        outPos.set(op);
    }

    public void uncompress(int[] inBuf, IntWrapper inPos, int inLen, int[] outBuf, IntWrapper outPos)
    {
        if (inLen == 0)
        {
            return;
        }

        int outLen = inBuf[inPos.get()];
        inPos.increment();

        var ctx = new DeltaZigzagEncoding.Decoder(0);
        int[] work = new int[BLOCK_LENGTH];

        int ip = inPos.get();
        int op = outPos.get();
        int outPosLast = op + outLen;
        for (; op < outPosLast; op += BLOCK_LENGTH)
        {
            int n = inBuf[ip++];
            ip += unpack(inBuf, ip, work, 0, (n >> 24) & 0x3F);
            ip += unpack(inBuf, ip, work, 32, (n >> 16) & 0x3F);
            ip += unpack(inBuf, ip, work, 64, (n >> 8) & 0x3F);
            ip += unpack(inBuf, ip, work, 96, (n >> 0) & 0x3F);
            ctx.decodeArray(work, 0, BLOCK_LENGTH, outBuf, op);
        }

        outPos.add(outLen);
        inPos.set(ip);
    }

    private static int pack(int[] inBuf, int inOff, int[] outBuf, int outOff, int validBits)
    {
        BitPacking.fastpackwithoutmask(inBuf, inOff, outBuf, outOff, validBits);
        return validBits;
    }

    private static int unpack(int[] inBuf, int inOff, int[] outBuf, int outOff, int validBits)
    {
        BitPacking.fastunpack(inBuf, inOff, outBuf, outOff, validBits);
        return validBits;
    }

    public override string ToString()
    {
        return nameof(DeltaZigzagBinaryPacking);
    }
}