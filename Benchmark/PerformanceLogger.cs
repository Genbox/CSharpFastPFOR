/**
 * This code is released under the
 * Apache License Version 2.0 http://www.apache.org/licenses/.
 */

/**
 * PerformanceLogger for IntegerCODEC.
 * 
 * @author MURAOKA Taro http://github.com/koron
 */

using System.Diagnostics;

namespace CSharpFastPFOR.Benchmarks
{
    public class PerformanceLogger
    {
        public class Timer
        {
            private Stopwatch sw = new Stopwatch();

            public void start()
            {
                sw.Start();
            }

            public long end()
            {
                sw.Stop();
                return sw.ElapsedMilliseconds * 1000 * 1000;
            }

            public long getDuration()
            {
                return sw.ElapsedMilliseconds * 1000 * 1000;
            }
        }

        public Timer compressionTimer = new Timer();
        public Timer decompressionTimer = new Timer();

        private long originalSize = 0;

        private long compressedSize = 0;

        public long addOriginalSize(long value)
        {
            return this.originalSize += value;
        }

        public long addCompressedSize(long value)
        {
            return this.compressedSize += value;
        }

        long getOriginalSize()
        {
            return this.originalSize;
        }

        long getCompressedSize()
        {
            return this.compressedSize;
        }

        public double getBitPerInt()
        {
            return this.compressedSize * 32.0 / this.originalSize;
        }

        private static double getMiS(long size, long nanoTime)
        {
            return (size * 1e-6) / (nanoTime * 1.0e-9);
        }

        public double getCompressSpeed()
        {
            return getMiS(this.originalSize, this.compressionTimer.getDuration());
        }

        public double getDecompressSpeed()
        {
            return getMiS(this.originalSize, this.decompressionTimer.getDuration());
        }
    }
}