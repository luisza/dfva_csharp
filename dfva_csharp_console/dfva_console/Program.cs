using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using dfva_csharp.dfva;

namespace dfva_csharp_console
{
    class MainClass
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger("dfva_csharp");
        public static void Main(string[] args)
        {
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX)
    ? Environment.GetEnvironmentVariable("HOME")
    : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            
            log.Info("Recuerde crear el archivo test.pdf en su carpeta personal");
            string testpdf = Path.Combine(homePath, "test.pdf");

            log.Info("INICIANDO DEMO");
            Settings settings = new Settings();
            settings.load();
            var client = new Client(settings);

            log.Info("---- PERSONA CONECTADA ----");
            Console.WriteLine(client.suscriptor_connected("0402120119"));

            log.Info("---- FIRMANDO UN PDF ----");
            byte[] document =
            Encoding.UTF8.GetBytes(File.OpenText(testpdf).ReadToEnd());
            
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


            log.Info("--- AUTENTICANDO UNA PERSONA ---");
            var response2 = client.authenticate("0403210121");
            object code2;
            response.TryGetValue("id_transaction", out code2);

            response = client.authenticate_check(Convert.ToString(code2));
            foreach (KeyValuePair<string, object> data in response2)
            {
                Console.WriteLine(data.Key + " = " + data.Value);
            }
            Console.WriteLine("Delete: " + client.authenticate_delete(Convert.ToString(code2)));

			//client.inspect = true;
            log.Info("--- VALIDANDO DOCUMENTOS ---");
            var response3 = client.validate(document, "document", "pdf");
            //var response4 = client.validate(document, "certificate");
            foreach (KeyValuePair<string, object> data in response3)
            {
                Console.WriteLine(data.Key + " = " + data.Value);
            }
            
            log.Info("--- VALIDANDO CERTIFICADOS ---");
            var response4 = client.validate(document, "certificate");
            foreach (KeyValuePair<string, object> data in response4)
            {
                Console.WriteLine(data.Key + " = " + data.Value);
            }


        }
    }
}