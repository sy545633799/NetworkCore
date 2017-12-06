using System;
using System.Text;
using System.Security.Cryptography;

public static class MD5Crypto
{
    /// <summary>
    /// MD5 加密字符串
    /// </summary>
    /// <param name="rawPass">源字符串</param>
    /// <returns>加密后字符串</returns>
    public static string MD5Encoding(string rawPass)
    {
        // 创建MD5类的默认实例：MD5CryptoServiceProvider
        MD5 md5 = MD5.Create();
        byte[] bs = Encoding.UTF8.GetBytes(rawPass);
        byte[] hs = md5.ComputeHash(bs);

        StringBuilder sb = new StringBuilder();

        // 以十六进制格式格式化
        foreach (byte b in hs) sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}