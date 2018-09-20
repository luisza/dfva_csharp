using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using dfva_csharp.dfva;

namespace dfva_csharp_console
{
    class MainClass
    {
        public static void Main(string[] args)
        {
			Settings settings = new Settings();
			settings.load();
			var client = new Client(settings);
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
            response.TryGetValue("id_transaction", out code);

            response = client.authenticate_check(Convert.ToString(code));
            foreach (KeyValuePair<string, object> data in response)
            {
                Console.WriteLine(data.Key + " = " + data.Value);
            }
            Console.WriteLine("Delete: " + client.authenticate_delete(Convert.ToString(code)));


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