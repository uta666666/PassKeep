using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Common {
    public class FileManager {
        public static void WriteWithEncrypt(string outFilePath, string password, string sourceString) {
            //AesCryptoServiceProviderオブジェクトの作成
            //AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            AesManaged aes = new AesManaged();

            //※AESはブロックサイズ、キー長ともに128bit
            aes.BlockSize = 128; //ブロックサイズ
            aes.KeySize = 128; //キー最大長
            aes.Mode = CipherMode.CBC; //CBCモード
            aes.Padding = PaddingMode.PKCS7; //パディングモード

            var deriveBytes = new Rfc2898DeriveBytes(password, 16);
            var salt = new byte[16];
            salt = deriveBytes.Salt;
            byte[] bytesKey = deriveBytes.GetBytes(16);

            aes.Key = bytesKey; //処理済みパスワードをセット
            aes.GenerateIV();

            //AES暗号化オブジェクトの作成
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (FileStream outfs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write))
            using (CryptoStream cse = new CryptoStream(outfs, encryptor, CryptoStreamMode.Write)) {
                outfs.Write(salt, 0, 16);   // salt をファイル先頭に埋め込む
                outfs.Write(aes.IV, 0, 16); // 次にIVもファイルに埋め込む

                byte[] buffer = new byte[4096];
                int len;
                byte[] strBytes = Encoding.UTF8.GetBytes(sourceString);
                using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress)) //圧縮
                using (MemoryStream ms = new MemoryStream(strBytes)) {
                    ds.Write(Encoding.UTF8.GetBytes("success"), 0, 7);
                    while ((len = ms.Read(buffer, 0, 4096)) > 0) {
                        ds.Write(buffer, 0, len);
                    }
                }
            }
        }

        public static string ReadWithDecrypt(string filePath, string password) {
            try {
                int len;
                byte[] buffer = new byte[4096];

                //if (String.Compare(Path.GetExtension(filePath), ".enc", true) != 0) {
                //    //The file are not encrypted file! Decryption failed
                //    MessageBox.Show("暗号化されたファイルではありません！" + Environment.NewLine + "復号に失敗しました。",
                //        "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //    return (false); ;
                //}

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (AesManaged aes = new AesManaged()) {
                    aes.BlockSize = 128;              // BlockSize = 16bytes
                    aes.KeySize = 128;                // KeySize = 16bytes
                    aes.Mode = CipherMode.CBC;        // CBC mode
                    aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                    // salt
                    byte[] salt = new byte[16];
                    fs.Read(salt, 0, 16);

                    // Initilization Vector
                    byte[] iv = new byte[16];
                    fs.Read(iv, 0, 16);
                    aes.IV = iv;

                    // ivをsaltにしてパスワードを擬似乱数に変換
                    var deriveBytes = new Rfc2898DeriveBytes(password, salt);
                    byte[] bufferKey = deriveBytes.GetBytes(16);    // 16バイトのsaltを切り出してパスワードに変換
                    aes.Key = bufferKey;

                    //Decryption interface.
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                    using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))
                    using (MemoryStream outms = new MemoryStream()) {
                        using(MemoryStream tempms = new MemoryStream()) {
                            len = ds.Read(buffer, 0, 7);
                            tempms.Write(buffer, 0, len);
                            var a = Encoding.UTF8.GetString(tempms.ToArray());
                            if (a != "success") {
                                return string.Empty;
                            }
                        }

                        while ((len = ds.Read(buffer, 0, 4096)) > 0) {
                            outms.Write(buffer, 0, len);
                        }
                        return Encoding.UTF8.GetString(outms.ToArray());
                    }
                }
            } catch (CryptographicException) {
                return string.Empty;
            }
        }
    }
}
