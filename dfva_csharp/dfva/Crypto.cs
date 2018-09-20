using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Crypto.Modes;
using System.Security.Cryptography;

namespace dfva_csharp.dfva
{
    public class Crypto
    {
		private const int NONCE_LEN = 16;
        private const int MAC_LEN = 16;
        private Settings settings;
		public Crypto(Settings dfvasettings)
        {
			settings = dfvasettings;
        }

		private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
			return result.ToString().ToLower();
        }

		public string get_hash_sum(string data, string algorithm){

			return get_hash_sum(Encoding.UTF8.GetBytes(data), algorithm);
		}
      
		public string get_hash_sum(byte[] bdata, string algorithm)
        {
			
			byte[] computedHash = null;
			if(algorithm.Equals("sha256")){
				using (var sha = SHA256.Create())
                {
                    computedHash = sha.ComputeHash(bdata);
                }
			} else if (algorithm.Equals("sha384")) {
				using (var sha = SHA384.Create())
                {
                    computedHash = sha.ComputeHash(bdata);
                }
			} else if (algorithm.Equals("sha512")) { 
				using (var sha = SHA512.Create())
                {
                    computedHash = sha.ComputeHash(bdata);
                }
			}else{

				throw new Exception("No algorithm found should be sha256, sha384 or sha512");
			}
      
			string computed=GetStringFromHash(computedHash);
			//Console.WriteLine(computed);
			return computed;
        }

		private byte[] get_encrypted_session_key(byte[] session_key){
			int length = session_key.Length;
			PemReader pr = new PemReader(
                (StreamReader)File.OpenText(settings.publicKey)
            );
            RsaKeyParameters keys = (RsaKeyParameters)pr.ReadObject();

            // Pure mathematical RSA implementation
            // RsaEngine eng = new RsaEngine();

            // PKCS1 v1.5 paddings
            // Pkcs1Encoding eng = new Pkcs1Encoding(new RsaEngine());

            // PKCS1 OAEP paddings
            OaepEncoding eng = new OaepEncoding(new RsaEngine());
            eng.Init(true, keys);

			int blockSize = eng.GetInputBlockSize();
            List<byte> cipherTextBytes = new List<byte>();
            for (int chunkPosition = 0;
                chunkPosition < length;
                chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, length - chunkPosition);
                cipherTextBytes.AddRange(eng.ProcessBlock(
					session_key, chunkPosition, chunkSize
                ));
            }
			return cipherTextBytes.ToArray();
            			
		}

		public string encrypt(string message){
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(message);
			byte[] session_key = new byte[16];
			byte[] nonce = new byte[NONCE_LEN];

			Random rnd = new Random();
			rnd.NextBytes(session_key);
			rnd.NextBytes(nonce);
            
			byte[] session_byte = get_encrypted_session_key(session_key);

			EaxBlockCipher eax = new EaxBlockCipher(new AesEngine());

			AeadParameters parameters = new AeadParameters(
				new KeyParameter(session_key), eax.GetBlockSize() * 8, nonce, new byte[0]);
			eax.Init(true, parameters);

			byte[] intrDat = new byte[eax.GetOutputSize(plainTextBytes.Length)];
			int outOff = eax.ProcessBytes(plainTextBytes, 0, plainTextBytes.Length, intrDat, 0);
            outOff += eax.DoFinal(intrDat, outOff);
			byte[] mac = eax.GetMac();
            
			int finalsize = intrDat.Length - mac.Length;

			byte[] finalobj = new byte[session_byte.Length+nonce.Length+mac.Length+finalsize];
			int copypos = 0;
			Array.Copy(session_byte, 0, finalobj, copypos, session_byte.Length);
			copypos += session_byte.Length; 
			Array.Copy(nonce, 0, finalobj, copypos, nonce.Length);
			copypos += nonce.Length;
			Array.Copy(mac, 0, finalobj, copypos, mac.Length);
			copypos += mac.Length;
			Array.Copy(intrDat, 0, finalobj, copypos, finalsize);

			/*using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
					writer.Write(session_byte.ToArray());
					writer.Write(nonce);
					writer.Write(mac);
					writer.Write(intrDat.ToList().GetRange(0, finalsize).ToArray());
                }
                finalBytesToSend = stream.ToArray();

            }*/
			//get_hash_sum(finalobj, settings.algorithm);
			return Convert.ToBase64String(finalobj);
		}

		private byte[] get_plain_session_key(byte[] cipherTextBytes){
			
            PemReader pr = new PemReader(
				(StreamReader)File.OpenText(settings.privateKey)
            );
            
			RsaPrivateCrtKeyParameters keys = (RsaPrivateCrtKeyParameters)pr.ReadObject();
            

            // Pure mathematical RSA implementation
            // RsaEngine eng = new RsaEngine();

            // PKCS1 v1.5 paddings
            // Pkcs1Encoding eng = new Pkcs1Encoding(new RsaEngine());

            // PKCS1 OAEP paddings
            OaepEncoding eng = new OaepEncoding(new RsaEngine());
			eng.Init(false, keys);

            int length = cipherTextBytes.Length;
            int blockSize = eng.GetInputBlockSize();
            byte[] decipheredBytes = eng.ProcessBlock(cipherTextBytes, 0, length);
			return decipheredBytes;
         
		}
        
		public string decrypt(string data){
			byte[] bdata = Convert.FromBase64String(data);
			PemReader pr = new PemReader(
				(StreamReader)File.OpenText(settings.privateKey)
            );
           
			RsaKeyParameters keys = (RsaKeyParameters)pr.ReadObject();
			int key_size= keys.Modulus.BitLength / 8;
			byte[] enc_session_key = new byte[key_size];
            byte[] nonce = new byte[NONCE_LEN];
			byte[] mac = new byte[MAC_LEN];
			byte[] cDat = new byte[bdata.Length - key_size - NONCE_LEN - MAC_LEN];
			byte[] intrDat = new byte[MAC_LEN + cDat.Length];


			Array.Copy(bdata, 0, enc_session_key, 0, key_size);
			Array.Copy(bdata, key_size, nonce, 0, NONCE_LEN);
			Array.Copy(bdata, key_size+NONCE_LEN, mac, 0, MAC_LEN);
			Array.Copy(bdata, key_size + NONCE_LEN + MAC_LEN, cDat, 0, cDat.Length);

			Array.Copy(cDat, 0, intrDat, 0, cDat.Length);
			Array.Copy(mac, 0, intrDat, cDat.Length, MAC_LEN);

			byte[] session_key = get_plain_session_key(enc_session_key);
            
            EaxBlockCipher eax = new EaxBlockCipher(new AesEngine());

            AeadParameters parameters = new AeadParameters(
                new KeyParameter(session_key), eax.GetBlockSize() * 8, nonce, new byte[0]);

            eax.Init(false, parameters);
			int outOff = intrDat.Length;
			byte[] datOut = new byte[eax.GetOutputSize(outOff)];
            int resultLen = eax.ProcessBytes(intrDat, 0, outOff, datOut, 0);
			eax.DoFinal(datOut, resultLen);
            return Encoding.UTF8.GetString(datOut);
		}

    }
}
