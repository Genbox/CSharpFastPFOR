/*
 * This code is released under the
 * Apache License Version 2.0 http://www.apache.org/licenses/.
 */

/**
 * VariableByte with Delta+Zigzag Encoding.
 * 
 * @author MURAOKA Taro http://github.com/koron
 */

using Genbox.CSharpFastPFOR.Port;

namespace Genbox.CSharpFastPFOR;

public class DeltaZigzagVariableByte : IntegerCODEC
{
    public void compress(int[] inBuf, IntWrapper inPos, int inLen, int[] outBuf, IntWrapper outPos)
    {
        if (inLen == 0)
        {
            return;
        }

        ByteBuffer byteBuf = ByteBuffer.allocateDirect(inLen * 5 + 3);
        DeltaZigzagEncoding.Encoder ctx = new DeltaZigzagEncoding.Encoder(0);

        // Delta+Zigzag+VariableByte encoding.
        int ip = inPos.get();

        int inPosLast = ip + inLen;
        for (; ip < inPosLast; ++ip)
        {
            // Filter with delta+zigzag encoding.
            int n = ctx.encodeInt(inBuf[ip]);
            // Variable byte encoding.

            //PORT NOTE: The following IF statements are ported from a switch. Fall through switches are not allowed in C#
            int zeros = Integer.numberOfLeadingZeros(n);

            if (zeros < 4)
            {
                byteBuf.put((sbyte)(((int)((uint)n >> 28) & 0x7F) | 0x80));
            }

            if (zeros < 11)
            {
                byteBuf.put((sbyte)(((int)((uint)n >> 21) & 0x7F) | 0x80));
            }

            if (zeros < 18)
            {
                byteBuf.put((sbyte)(((int)((uint)n >> 14) & 0x7F) | 0x80));
            }

            if (zeros < 25)
            {
                byteBuf.put((sbyte)(((int)((uint)n >> 7) & 0x7F) | 0x80));
            }

            byteBuf.put((sbyte)((uint)n & 0x7F));
        }

        // Padding buffer to considerable as IntBuffer.
        for (int i = (4 - (byteBuf.position() % 4)) % 4; i > 0; --i)
        {
            unchecked
            {
                byteBuf.put((sbyte)(0x80));
            }
        }

        int outLen = byteBuf.position() / 4;
        byteBuf.flip();
        IntBuffer intBuf = byteBuf.asIntBuffer();
        /*
     * Console.WriteLine(String.format(
     * "inLen=%d pos=%d limit=%d outLen=%d outBuf.len=%d", inLen,
     * intBuf.position(), intBuf.limit(), outLen, outBuf.Length));
     */
        intBuf.get(outBuf, outPos.get(), outLen);
        inPos.add(inLen);
        outPos.add(outLen);
    }

    public void uncompress(int[] inBuf, IntWrapper inPos, int inLen, int[] outBuf, IntWrapper outPos)
    {
        DeltaZigzagEncoding.Decoder ctx = new DeltaZigzagEncoding.Decoder(0);

        int ip = inPos.get();
        int op = outPos.get();
        int vbcNum = 0, vbcShift = 24; // Varialbe Byte Context.

        int inPosLast = ip + inLen;
        while (ip < inPosLast)
        {
            // Fetch a byte value.
            int n = (int)((uint)inBuf[ip] >> vbcShift) & 0xFF;
            if (vbcShift > 0)
            {
                vbcShift -= 8;
            }
            else
            {
                vbcShift = 24;
                ip++;
            }
            // Decode variable byte and delta+zigzag.
            vbcNum = (vbcNum << 7) + (n & 0x7F);
            if ((n & 0x80) == 0)
            {
                outBuf[op++] = ctx.decodeInt(vbcNum);
                vbcNum = 0;
            }
        }

        outPos.set(op);
        inPos.set(inPosLast);
    }

    public override string ToString()
    {
        return nameof(DeltaZigzagVariableByte);
    }
}