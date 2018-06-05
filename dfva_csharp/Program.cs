using System;
using dfva_csharp.Properties;
using System.Collections.Generic;
using Org.BouncyCastle.OpenSsl;
using System.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using System.Text;

namespace dfva_csharp
{
    class MainClass
    {
		public static void Main(string[] args)
		{
			var client = new Client(new Settings());
			/* byte[] document = 
			Encoding.UTF8.GetBytes(File.OpenText("test.pdf").ReadToEnd());
			
			var response = client.sign("0403210121",
						document,
						"pdf",
						"document test"
					   );

			object code;
            response.TryGetValue("id_transaction", out code);
			response = client.sign_check(Convert.ToString(code));
			foreach (KeyValuePair<string, object> data in response)
            {
                Console.WriteLine(data.Key + " = " + data.Value);
            }
			Console.WriteLine("Delete: " + client.sign_delete(Convert.ToString(code)));
            */

			var response = client.authenticate("0403210121");
			object code;
			response.TryGetValue("id_transaction",out code);
         
			response = client.authenticate_check(Convert.ToString(code));
			foreach (KeyValuePair<string, object> data in response)
            {
				Console.WriteLine(data.Key+" = "+data.Value);
            }
			Console.WriteLine("Delete: "+client.authenticate_delete(Convert.ToString(code)));
            
            
			//var response = client.validate(document, "document", "pdf");
			/** var response = client.validate(document, "certificate");
            foreach (KeyValuePair<string, object> data in response)
            {
                Console.WriteLine(data.Key + " = " + data.Value);
            }*/

			//Console.WriteLine(client.suscriptor_connected("0402120119"));
        }
    }
}
