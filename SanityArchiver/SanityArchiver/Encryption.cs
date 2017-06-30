using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace SanityArchiver
{
    public class Encryption
    {
        public static string Base64Encode(string plainText)
        {
            var salt = new byte[] { 1, 2, 23, 234, 37, 48, 134, 63, 248, 4 };
            const int Iterations = 9872;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(plainText, salt, Iterations))
                return Encoding.UTF8.GetString(rfc2898DeriveBytes.GetBytes(8));
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public void Crypt(string inputFilename, string outputFilename, string sKey)
        {
            sKey = Base64Encode(sKey);
            FileStream fsInput = new FileStream(inputFilename,
               FileMode.Open,
               FileAccess.Read);

            FileStream fsEncrypted = new FileStream(outputFilename,
               FileMode.Create,
               FileAccess.Write);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted,
               desencrypt,
               CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        public void Decrypt(string inputFilename,
           string outputFilename,
           string sKey)
        {
            sKey = Base64Encode(sKey);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            FileStream fsread = new FileStream(inputFilename,
               FileMode.Open,
               FileAccess.Read);

            ICryptoTransform desdecrypt = DES.CreateDecryptor();
            CryptoStream cryptostreamDecr = new CryptoStream(fsread,
               desdecrypt,
               CryptoStreamMode.Read);
            StreamWriter fsDecrypted = new StreamWriter(outputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }
    }

}