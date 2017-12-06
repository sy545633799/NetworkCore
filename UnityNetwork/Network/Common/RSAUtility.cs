using System;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using System.IO;


public static class RSAUtility
{
    /// <summary>
    /// 将pem格式公钥(1024 or 2048)转换为RSAParameters
    /// </summary>
    /// <param name="pemFileConent">pem公钥内容</param>
    /// <returns>转换得到的RSAParamenters</returns>

    public static RSAParameters ConvertFromPemPublicKey(string pemFileConent)
    {
        if (string.IsNullOrEmpty(pemFileConent))
        {
            throw new ArgumentNullException("pemFileConent", "This arg cann't be empty.");
        }

        pemFileConent = pemFileConent.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "");
        byte[] keyData = Convert.FromBase64String(pemFileConent);
        bool keySize1024 = (keyData.Length == 162);
        bool keySize2048 = (keyData.Length == 294);
        if (!(keySize1024 || keySize2048))
        {
            throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
        }

        byte[] pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
        byte[] pemPublicExponent = new byte[3];

        Array.Copy(keyData, (keySize1024 ? 29 : 33), pemModulus, 0, (keySize1024 ? 128 : 256));
        Array.Copy(keyData, (keySize1024 ? 159 : 291), pemPublicExponent, 0, 3);

        RSAParameters para = new RSAParameters();
        para.Modulus = pemModulus;
        para.Exponent = pemPublicExponent;

        return para;
    }


    /// <summary>
    /// 将pem格式私钥(1024 or 2048)转换为RSAParameters
    /// </summary>
    /// <param name="pemFileConent">pem私钥内容</param>
    /// <returns>转换得到的RSAParamenters</returns>
    public static RSAParameters ConvertFromPemPrivateKey(string pemFileConent)
    {
        if (string.IsNullOrEmpty(pemFileConent))
        {
            throw new ArgumentNullException("pemFileConent", "This arg cann't be empty.");
        }

        pemFileConent = pemFileConent.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "").Replace("\r", "");

        byte[] keyData = Convert.FromBase64String(pemFileConent);

        bool keySize1024 = (keyData.Length == 609 || keyData.Length == 610);
        bool keySize2048 = (keyData.Length == 1190 || keyData.Length == 1192);

        if (!(keySize1024 || keySize2048))
        {
            throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
        }

        int index = (keySize1024 ? 11 : 12);

        byte[] pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
        Array.Copy(keyData, index, pemModulus, 0, pemModulus.Length);

        index += pemModulus.Length;
        index += 2;

        byte[] pemPublicExponent = new byte[3];
        Array.Copy(keyData, index, pemPublicExponent, 0, 3);

        index += 3;
        index += 4;
        if ((int)keyData[index] == 0)
        {
            index++;
        }

        byte[] pemPrivateExponent = (keySize1024 ? new byte[128] : new byte[256]);
        Array.Copy(keyData, index, pemPrivateExponent, 0, pemPrivateExponent.Length);

        index += pemPrivateExponent.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));

        byte[] pemPrime1 = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemPrime1, 0, pemPrime1.Length);

        index += pemPrime1.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));

        byte[] pemPrime2 = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemPrime2, 0, pemPrime2.Length);

        index += pemPrime2.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));

        byte[] pemExponent1 = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemExponent1, 0, pemExponent1.Length);


        index += pemExponent1.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));

        byte[] pemExponent2 = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemExponent2, 0, pemExponent2.Length);

        index += pemExponent2.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));

        byte[] pemCoefficient = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemCoefficient, 0, pemCoefficient.Length);

        RSAParameters para = new RSAParameters();
        para.Modulus = pemModulus;
        para.Exponent = pemPublicExponent;
        para.D = pemPrivateExponent;
        para.P = pemPrime1;
        para.Q = pemPrime2;
        para.DP = pemExponent1;
        para.DQ = pemExponent2;
        para.InverseQ = pemCoefficient;

        return para;
    }


    // 使用公钥加密数据
    public static byte[] Encrypt(byte[] toEncrypt, string pub_key)
    {
        var param = ConvertFromPemPublicKey(pub_key);

        var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(param);

        return rsa.Encrypt(toEncrypt, false);
    }

    // 使用私钥解密数据
    public static byte[] Decrypt(byte[] toDecrypt, string pub_key) 
    {
        var param = ConvertFromPemPrivateKey(pub_key);

        var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(param);

        return rsa.Decrypt(toDecrypt, false);
    }
}

public class RSACrypto : IEncryptor
{
    private RSACryptoServiceProvider mrsa;

    public RSACrypto( string pub_key ) 
    {
        mrsa = new RSACryptoServiceProvider();
        mrsa.ImportParameters(RSAUtility.ConvertFromPemPublicKey(pub_key));
    }

    public byte[] Encrypt(byte[] toEncrypt)
    {
        return mrsa.Encrypt(toEncrypt, false);
    }

    public byte[] Decrypt(byte[] toDecrypt)
    { 
        throw new NotImplementedException("There is no private key.");
    }
}