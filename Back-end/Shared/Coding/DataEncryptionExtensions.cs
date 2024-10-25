using System.Security.Cryptography;
using System.Text;

namespace Utility.Coding
{
    public static class DataEncryptionExtensions
    {
        #region [Hashing Extension]
        public static string ToSHA256Hash(this string password, string? saltKey, int length = 30)
        {
            var sha256 = SHA256.Create();
            byte[] encryptedSHA256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
            sha256.Clear();

            return Convert.ToBase64String(encryptedSHA256).Substring(0, length);
        }

        public static string ToSHA256Hash(this string password)
        {
            string? saltKey = "DONOTTOUCHOURPASSWORD!!!";
            var sha256 = SHA256.Create();
            byte[] encryptedSHA256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
            sha256.Clear();

            return Convert.ToBase64String(encryptedSHA256).Substring(0, 20);
        }

        public static string ToSHA512Hash(this string password, string? saltKey)
        {
            SHA512Managed sha512 = new();
            byte[] encryptedSHA512 = sha512.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
            sha512.Clear();

            return Convert.ToBase64String(encryptedSHA512).Substring(0, 30);
        }

        public static string ToMd5Hash(this string password, string? saltKey)
        {
            using (var md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString().Substring(0, 30);
            }
        }

        public static string ToMd5Hash(this string password)
        {
            using (var md5 = MD5.Create())
            {
                string? saltKey = "KATH&DATH2&LUCIA";
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString().Substring(0, 30);
            }
        }

        #endregion


        public static string Encrypt(string plainText, in string key = "DONOTTOUCHOURPASSWORD!!!", in string iv = "1234567890123456")
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                using StreamWriter sw = new(cs);
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText, in string key = "DONOTTOUCHOURPASSWORD!!!", in string iv = "1234567890123456")
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            using MemoryStream ms = new(Convert.FromBase64String(cipherText));
            using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// Hàm này tạo ra một chuỗi ngẫu nhiên gồm các ký tự chữ cái và số với độ dài mặc định là 6.
        /// </summary>
        /// <param name="length">Độ dài của chuỗi cần tạo. Mặc định là 6.</param>
        /// <returns>
        /// Một chuỗi ngẫu nhiên gồm các ký tự chữ cái (chữ hoa, chữ thường) và số.
        /// </returns>
        public static string GenerateRandomString(int length = 6)
        {
            // Chuỗi chứa các ký tự chữ cái và số
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            // Đối tượng Random để sinh số ngẫu nhiên
            Random random = new();

            // Sinh ra chuỗi ngẫu nhiên từ tập hợp các ký tự
            string randomString = new(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }


        // For MOMO API

        // Not mine
        public static string HmacSHA256(string inputData, string key)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            string hex = BitConverter.ToString(hashmessage);
            hex = hex.Replace("-", "").ToLower();
            return hex;
        }

        // Mine
        public static string SignSHA256(string data, string key)
        {
            var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        //public static string EncryptRSA(string data, string publicKeyHex)
        //{
        //    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        //    RsaKeyParameters publicKey = CreateRsaKeyParametersFromHex(publicKeyHex);

        //    var encryptEngine = new Pkcs1Encoding(new RsaEngine());
        //    encryptEngine.Init(true, publicKey);

        //    var encryptedData = encryptEngine.ProcessBlock(dataBytes, 0, dataBytes.Length);
        //    return Convert.ToBase64String(encryptedData);
        //}

        //private static RsaKeyParameters CreateRsaKeyParametersFromHex(string publicKeyHex)
        //{
        //    byte[] publicKeyBytes = ConvertHexToBytes(publicKeyHex);

        //    // Convert the byte array to BigInteger
        //    var modulus = new Org.BouncyCastle.Math.BigInteger(1, publicKeyBytes);
        //    var exponent = Org.BouncyCastle.Math.BigInteger.ValueOf(65537);

        //    return new RsaKeyParameters(false, modulus, exponent);
        //}

        private static byte[] ConvertHexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex; // Pad with a leading zero if the length is odd
            }

            return Enumerable.Range(0, hex.Length / 2)
                             .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                             .ToArray();
        }

        private static string CreateRsaPublicKeyXml(string base64Key)
        {
            return $"<RSAKeyValue><Modulus>{base64Key}</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        }
    }

}
