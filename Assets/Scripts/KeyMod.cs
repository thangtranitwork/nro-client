using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
public class KeyMod
{
    public static byte[] datakey;
    public static byte[] key;
    public static byte[] iv;
    public static string ConvertBytesToString(byte[] bytes)
    {
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static string StringSend()
    {
        string resultString = ConvertBytesToString(datakey);
        Debug.LogError(DecryptStringAES(resultString, key, iv));
        return DecryptStringAES(resultString, key, iv);
    }
    public static string DecryptStringAES(string cipherTextBase64, byte[] Key, byte[] IV)
    {
        if (string.IsNullOrEmpty(cipherTextBase64))
            throw new ArgumentNullException(nameof(cipherTextBase64));
        if (Key == null || Key.Length == 0)
            throw new ArgumentNullException(nameof(Key));
        if (IV == null || IV.Length == 0)
            throw new ArgumentNullException(nameof(IV));

        byte[] cipherText = Convert.FromBase64String(cipherTextBase64);
        string plaintext = null;
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }
}


