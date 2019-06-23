/**
 * This code is released under the
 * Apache License Version 2.0 http://www.apache.org/licenses/.
 *
 * (c) Daniel Lemire, http://lemire.me/en/
 */

/**
 * Interface describing a CODEC to compress integers to bytes.
 * 
 * "Integrated" means that it uses differential coding.
 * 
 * @author Daniel Lemire
 * 
 */
namespace CSharpFastPFOR.Differential
{
    public interface IntegratedByteIntegerCODEC : ByteIntegerCODEC
    {
    }
}