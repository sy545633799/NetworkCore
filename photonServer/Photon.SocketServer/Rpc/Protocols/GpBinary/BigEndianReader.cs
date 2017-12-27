using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Photon.SocketServer.Rpc.Protocols.GpBinary
{
    internal class BigEndianReader
    {
        // Methods
        public static bool TryReadBoolean(byte[] buf, ref int pos, out bool value)
        {
            if (pos >= buf.Length)
            {
                value = false;
                return false;
            }
            value = buf[pos++] != 0;
            return true;
        }

        public static bool TryReadBoolean(byte[] buf, ref int pos, out object value)
        {
            if (pos >= buf.Length)
            {
                value = false;
                return false;
            }
            value = buf[pos++] != 0;
            return true;
        }

        public static unsafe bool TryReadBooleanArray(byte[] buf, ref int pos, int count, out bool[] value)
        {
            if (count == 0)
            {
                value = new bool[0];
                return true;
            }
            if (pos > (buf.Length - count))
            {
                value = null;
                return false;
            }
            value = new bool[count];
            fixed (byte* numRef = &(buf[pos]))
            {
                fixed (bool* flagRef = value)
                {
                    byte* numPtr = numRef;
                    for (int i = 0; i < count; i++)
                    {
                        flagRef[i] = numPtr[i] != 0;
                    }
                }
            }
            pos += count;
            return true;
        }

        public static bool TryReadBooleanArray(byte[] buf, ref int pos, int count, out object value)
        {
            bool[] flagArray;
            bool flag = TryReadBooleanArray(buf, ref pos, count, out flagArray);
            value = flagArray;
            return flag;
        }

        public static bool TryReadByte(byte[] buf, ref int pos, out byte value)
        {
            if (pos >= buf.Length)
            {
                value = 0;
                return false;
            }
            value = buf[pos++];
            return true;
        }

        public static bool TryReadByte(byte[] buf, ref int pos, out object value)
        {
            if (pos >= buf.Length)
            {
                value = 0;
                return false;
            }
            value = buf[pos++];
            return true;
        }

        public static bool TryReadByteArray(byte[] buf, ref int pos, int count, out byte[] value)
        {
            if (pos > (buf.Length - count))
            {
                value = null;
                return false;
            }
            value = new byte[count];
            Buffer.BlockCopy(buf, pos, value, 0, count);
            pos += count;
            return true;
        }

        public static bool TryReadByteArray(byte[] buf, ref int pos, int count, out object value)
        {
            if (pos > (buf.Length - count))
            {
                value = null;
                return false;
            }
            byte[] dst = new byte[count];
            Buffer.BlockCopy(buf, pos, dst, 0, count);
            pos += count;
            value = dst;
            return true;
        }

        public static unsafe bool TryReadDouble(byte[] buf, ref int pos, out double value)
        {
            long num;
            if (TryReadInt64(buf, ref pos, out num))
            {
                value = *((double*)&num);
                return true;
            }
            value = 0.0;
            return false;
        }

        public static unsafe bool TryReadDouble(byte[] buf, ref int pos, out object value)
        {
            long num;
            if (TryReadInt64(buf, ref pos, out num))
            {
                value = *((double*)&num);
                return true;
            }
            value = 0;
            return false;
        }

        public static unsafe bool TryReadDoubleArray(byte[] buf, ref int pos, int count, out double[] value)
        {
            if (count == 0)
            {
                value = new double[0];
                return true;
            }
            int num = count * 8;
            if (pos > (buf.Length - num))
            {
                value = null;
                return false;
            }
            value = new double[count];
            fixed (byte* numRef = &(buf[pos]))
            {
                fixed (double* numRef2 = value)
                {
                    byte* numPtr = numRef;
                    for (int i = 0; i < count; i++)
                    {
                        long num3 = (((((((numPtr[0] << 0x38) | (numPtr[1] << 0x30)) | (numPtr[2] << 40)) | (numPtr[3] << 0x20)) | (numPtr[4] << 0x18)) | (numPtr[5] << 0x10)) | (numPtr[6] << 8)) | numPtr[7];
                        numRef2[i * 8] = *((double*)&num3);
                        numPtr += 8;
                    }
                }
            }
            pos += num;
            return true;
        }

        public static bool TryReadDoubleArray(byte[] buf, ref int pos, int count, out object value)
        {
            double[] numArray;
            bool flag = TryReadDoubleArray(buf, ref pos, count, out numArray);
            value = numArray;
            return flag;
        }

        public static bool TryReadInt16(byte[] buf, ref int pos, out short value)
        {
            if (pos > (buf.Length - 2))
            {
                value = 0;
                return false;
            }
            int index = pos;
            value = (short)((buf[index] << 8) | buf[index + 1]);
            pos += 2;
            return true;
        }

        public static bool TryReadInt16(byte[] buf, ref int pos, out object value)
        {
            if (pos > (buf.Length - 2))
            {
                value = 0;
                return false;
            }
            int index = pos;
            value = (short)((buf[index] << 8) | buf[index + 1]);
            pos += 2;
            return true;
        }

        public static unsafe bool TryReadInt16Array(byte[] buf, ref int pos, int count, out short[] value)
        {
            if (count == 0)
            {
                value = new short[0];
                return true;
            }
            int num = count * 2;
            if (pos > (buf.Length - num))
            {
                value = null;
                return false;
            }
            value = new short[count];
            fixed (byte* numRef = &(buf[pos]))
            {
                fixed (short* numRef2 = value)
                {
                    byte* numPtr = numRef;
                    for (int i = 0; i < count; i++)
                    {
                        numRef2[i] = (short)((numPtr[0] << 8) | numPtr[1]);
                        numPtr += 2;
                    }
                }
            }
            pos += num;
            return true;
        }

        public static bool TryReadInt16Array(byte[] buf, ref int pos, int count, out object value)
        {
            short[] numArray;
            bool flag = TryReadInt16Array(buf, ref pos, count, out numArray);
            value = numArray;
            return flag;
        }

        public static bool TryReadInt32(byte[] buf, ref int pos, out int value)
        {
            int index = pos;
            if (index > (buf.Length - 4))
            {
                value = 0;
                return false;
            }
            value = (((buf[index] << 0x18) | (buf[index + 1] << 0x10)) | (buf[index + 2] << 8)) | buf[index + 3];
            pos += 4;
            return true;
        }

        public static bool TryReadInt32(byte[] buf, ref int pos, out object value)
        {
            if (pos > (buf.Length - 4))
            {
                value = 0;
                return false;
            }
            value = (((buf[pos] << 0x18) | (buf[pos + 1] << 0x10)) | (buf[pos + 2] << 8)) | buf[pos + 3];
            pos += 4;
            return true;
        }

        public static unsafe bool TryReadInt32Array(byte[] buf, ref int pos, int count, out int[] value)
        {
            if (count == 0)
            {
                value = new int[0];
                return true;
            }
            int num = count * 4;
            if (pos > (buf.Length - num))
            {
                value = null;
                return false;
            }
            value = new int[count];
            fixed (byte* numRef = &(buf[pos]))
            {
                fixed (int* numRef2 = value)
                {
                    byte* numPtr = numRef;
                    for (int i = 0; i < count; i++)
                    {
                        numRef2[i] = (((numPtr[0] << 0x18) | (numPtr[1] << 0x10)) | (numPtr[2] << 8)) | numPtr[3];
                        numPtr += 4;
                    }
                }
            }
            pos += num;
            return true;
        }

        public static bool TryReadInt32Array(byte[] buf, ref int pos, int count, out object value)
        {
            int[] numArray;
            bool flag = TryReadInt32Array(buf, ref pos, count, out numArray);
            value = numArray;
            return flag;
        }

        public static bool TryReadInt64(byte[] buf, ref int pos, out long value)
        {
            if (pos > (buf.Length - 8))
            {
                value = 0L;
                return false;
            }
            value = (((((((buf[pos] << 0x38) | (buf[pos + 1] << 0x30)) | (buf[pos + 2] << 40)) | (buf[pos + 3] << 0x20)) | (buf[pos + 4] << 0x18)) | (buf[pos + 5] << 0x10)) | (buf[pos + 6] << 8)) | buf[pos + 7];
            pos += 8;
            return true;
        }

        public static bool TryReadInt64(byte[] buf, ref int pos, out object value)
        {
            if (pos > (buf.Length - 8))
            {
                value = 0;
                return false;
            }
            value = (((((((buf[pos] << 0x38) | (buf[pos + 1] << 0x30)) | (buf[pos + 2] << 40)) | (buf[pos + 3] << 0x20)) | (buf[pos + 4] << 0x18)) | (buf[pos + 5] << 0x10)) | (buf[pos + 6] << 8)) | buf[pos + 7];
            pos += 8;
            return true;
        }

        public static unsafe bool TryReadInt64Array(byte[] buf, ref int pos, int count, out long[] value)
        {
            if (count == 0)
            {
                value = new long[0];
                return true;
            }
            int num = count * 8;
            if (pos > (buf.Length - num))
            {
                value = null;
                return false;
            }
            value = new long[count];
            fixed (byte* numRef = &(buf[pos]))
            {
                fixed (long* numRef2 = value)
                {
                    byte* numPtr = numRef;
                    for (int i = 0; i < count; i++)
                    {
                        numRef2[i] = (((((((numPtr[0] << 0x38) | (numPtr[1] << 0x30)) | (numPtr[2] << 40)) | (numPtr[3] << 0x20)) | (numPtr[4] << 0x18)) | (numPtr[5] << 0x10)) | (numPtr[6] << 8)) | numPtr[7];
                        numPtr += 8;
                    }
                }
            }
            pos += num;
            return true;
        }

        public static bool TryReadInt64Array(byte[] buf, ref int pos, int count, out object value)
        {
            long[] numArray;
            bool flag = TryReadInt64Array(buf, ref pos, count, out numArray);
            value = numArray;
            return flag;
        }

        public static unsafe bool TryReadSingle(byte[] buf, ref int pos, out object value)
    {
        int num;
        if (TryReadInt32(buf, ref pos, out num))
        {
            value = *((float*)&num);
            return true;
        }
        value = 0;
        return false;
    }

        public static unsafe bool TryReadSingle(byte[] buf, ref int pos, out float value)
    {
        int num;
        if (TryReadInt32(buf, ref pos, out num))
        {
            value = *((float*) &num);
            return true;
        }
        value = 0f;
        return false;
    }

        public static unsafe bool TryReadSingleArray(byte[] buf, ref int pos, int count, out float[] value)
        {
            if (count == 0)
            {
                value = new float[0];
                return true;
            }
            int num = count * 4;
            if (pos > (buf.Length - num))
            {
                value = null;
                return false;
            }
            value = new float[count];
            fixed (byte* numRef = &(buf[pos]))
            {
                fixed (float* numRef2 = value)
                {
                    byte* numPtr = numRef;
                    for (int i = 0; i < count; i++)
                    {
                        int num3 = (((numPtr[0] << 0x18) | (numPtr[1] << 0x10)) | (numPtr[2] << 8)) | numPtr[3];
                        numRef2[i * 4] = *((float*)&num3);
                        numPtr += 4;
                    }
                }
            }
            pos += num;
            return true;
        }

        public static bool TryReadSingleArray(byte[] buf, ref int pos, int count, out object value)
        {
            float[] numArray;
            bool flag = TryReadSingleArray(buf, ref pos, count, out numArray);
            value = numArray;
            return flag;
        }

        public static bool TryReadString(byte[] buf, ref int pos, int byteCount, out object value)
        {
            if (pos > (buf.Length - byteCount))
            {
                value = null;
                return false;
            }
            value = Encoding.UTF8.GetString(buf, pos, byteCount);
            pos += byteCount;
            return true;
        }

        public static bool TryReadString(byte[] buf, ref int pos, int byteCount, out string value)
        {
            if (pos > (buf.Length - byteCount))
            {
                value = null;
                return false;
            }
            value = Encoding.UTF8.GetString(buf, pos, byteCount);
            pos += byteCount;
            return true;
        }
    }
}
