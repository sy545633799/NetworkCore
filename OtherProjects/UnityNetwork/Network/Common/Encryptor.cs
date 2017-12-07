using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public interface IEncryptor
{
    // 加密数据
    byte[] Encrypt(byte[] toEncrypt);

    // 解密数据
    byte[] Decrypt(byte[] toDecrypt);
}

