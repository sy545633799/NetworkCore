using System;
using System.Collections.Generic;
using System.Text;

public static class RC4Utility
{
    const string rc4_table = "1234567890ABCDEF<>?:{!@#$%^&*()}abcdef";

    public static byte[] GenRC4SecretKey( int seed )
    {
        var rand = new MersenneTwister(seed, 256);

        var key = new byte[256];

        for (var i = 0; i < 256; i++)
        {
            var index = rand.Next(0, rc4_table.Length - 1);

            key[i] = (byte)rc4_table[index];
        }

        return key;
    }

    public static byte[] Encrypt(byte[] data, byte[] key)
    {
        if (data == null || key == null) return null;
        Byte[] output = new Byte[data.Length];
        Int64 i = 0;
        Int64 j = 0;
        Byte[] mBox = GetKey(key, 256);
        // 加密
        for (Int64 offset = 0; offset < data.Length; offset++)
        {
            i = (i + 1) % mBox.Length;
            j = (j + mBox[i]) % mBox.Length;
            Byte temp = mBox[i];
            mBox[i] = mBox[j];
            mBox[j] = temp;
            Byte a = data[offset];
            //Byte b = mBox[(mBox[i] + mBox[j] % mBox.Length) % mBox.Length];
            // mBox[j] 一定比 mBox.Length 小，不需要在取模
            Byte b = mBox[(mBox[i] + mBox[j]) % mBox.Length];
            output[offset] = (Byte)((Int32)a ^ (Int32)b);
        }
        return output;
    }

    public static byte[] Decrypt(byte[] data, byte[] key)
    {
        return Encrypt(data, key);
    }

    /// <summary>
    /// 打乱密码
    /// </summary>
    /// <param name="pass">密码</param>
    /// <param name="kLen">密码箱长度</param>
    /// <returns>打乱后的密码</returns>
    static private Byte[] GetKey(Byte[] pass, Int32 kLen)
    {
        Byte[] mBox = new Byte[kLen];

        for (Int64 i = 0; i < kLen; i++)
        {
            mBox[i] = (Byte)i;
        }
        Int64 j = 0;
        for (Int64 i = 0; i < kLen; i++)
        {
            j = (j + mBox[i] + pass[i % pass.Length]) % kLen;
            Byte temp = mBox[i];
            mBox[i] = mBox[j];
            mBox[j] = temp;
        }
        return mBox;
    }

    public static void XORKeyStream(ref Byte[] bytes, Byte[] key)
    {
        Byte[] result = new byte[256];
        Byte[] s = new Byte[256];
        Byte[] k = new Byte[256];
        Byte temp;

        int i, j;
        int n = key.Length;
        for (i = 0; i < 256; i++)
        {
            s[i] = (Byte)i;
            //k[i] = key[i % key.GetLength(0)];
            k[i] = key[i % n];
        }

        j = 0;
        for (i = 0; i < 256; i++)
        {
            j = (j + s[i] + k[i]) % 256;
            temp = s[i];
            s[i] = s[j];
            s[j] = temp;
        }

        i = 0; j = 0;
        for (int x = 0; x < bytes.GetLength(0); x++)
        {
            i = (i + 1) % 256;
            j = (j + s[i]) % 256;
            temp = s[i];
            s[i] = s[j];
            s[j] = temp;

            int t = (s[i] + s[j]) % 256;
            bytes[x] ^= s[t];
        }
    }
}

public class RC4Cipher
{
    private uint[] s = new uint[256];
    private byte i, j;

    public RC4Cipher(byte[] key)
    {
        int k = key.Length;
        if (k < 1 || k > 256)
        {
            throw new Exception("error");
        }

        for (uint i = 0; i < 256; i++)
        {
            s[i] = i;
        }

        byte j = 0;
        uint t = 0;
        for (int i = 0; i < 256; i++)
        {
            j = (byte)(j + s[i] + key[i % k]);
            t = s[i];
            s[i] = s[j];
            s[j] = t;
        }
    }

    public void XORKeyStream(byte[] dst, int dstOffset, byte[] src, int srcOffset, int count)
    {
        if (count == 0)
            return;

        byte i = this.i;
        byte j = this.j;
        uint t = 0;
        for (int k = 0; k < count; k++)
        {
            i += 1;
            j = (byte)(s[i] + j);
            t = s[i];
            s[i] = s[j];
            s[j] = t;
            dst[k + dstOffset] = (byte)(src[k + srcOffset] ^ (byte)(s[(byte)(s[i] + s[j])]));
        }
        this.i = i;
        this.j = j;
    }
}


public class RC4Crypto : IEncryptor
{
    private byte[] mkey;

    public RC4Crypto( int seed )
    {
        mkey = RC4Utility.GenRC4SecretKey(seed);

        Console.WriteLine("RC4Crypto: {0}", System.Text.Encoding.Default.GetString(mkey));
    }

    public byte[] Encrypt(byte[] toEncrypt)
    {
        return RC4Utility.Encrypt(toEncrypt, this.mkey);
    }

    public byte[] Decrypt(byte[] toDecrypt)
    {
        return RC4Utility.Decrypt(toDecrypt, this.mkey);
    }
}