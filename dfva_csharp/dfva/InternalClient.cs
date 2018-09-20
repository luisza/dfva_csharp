using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;

namespace dfva_csharp.dfva
{
	public class InternalClient
	{
		private Settings settings;
		private Crypto crypto;
		public InternalClient(Settings dfvasettings)
		{
			settings = dfvasettings;
			crypto = new Crypto(dfvasettings);
		}

		public Dictionary<string, string> request_server(string serverurl, Dictionary<string, string> args)
		{
			var client = new RestClient(settings.baseUrl);
			var request = new RestRequest(serverurl, Method.POST);         
			request.AddJsonBody(args);
			request.AddHeader("Content-Type", "application/json");
			var response = client.Execute<Dictionary<string, string>>(request);
			return response.Data;
		}

		private string process_response(Dictionary<string, string> responses)
        {
            string dataenc = System.String.Empty;
			string datahash = System.String.Empty;
            responses.TryGetValue("data", out dataenc);
			responses.TryGetValue("data_hash", out datahash);
            string decrypted = crypto.decrypt(dataenc);
			string checksum  = crypto.get_hash_sum(decrypted, settings.algorithm);
			if( !checksum.Equals(datahash)){
				Dictionary<string, object> dev = new Dictionary<string, object> {
                {"code", "N/D"},
                {"status",  -2},
                {"identification", null},
                {"id_transaction", 0},
                {"sign_document", ""},
                {"expiration_datetime", ""},
                {"received_notification", true},
                {"status_text", "Problema de integridad, suma hash no es igual"}
                };
				decrypted = JsonConvert.SerializeObject(dev);
			}

            return decrypted;
        }
		protected Dictionary<string, object> authenticate(string identification)
		{
			Dictionary<string, string> data = new Dictionary<string, string> {
				{ "institution", settings.institution},
				{ "notification_url", settings.notificationURL },
				{"identification", identification},
				{"request_datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
			};
		
			string str_data = JsonConvert.SerializeObject(data);
			string edata = crypto.encrypt(str_data);
			string checksum = crypto.get_hash_sum(edata, settings.algorithm);
			Dictionary<string, string> args = new Dictionary<string, string>
            {
				{ "algorithm", settings.algorithm },
				{ "public_certificate", settings.publicCertificate },
				{ "institution", settings.institution},
				{ "data_hash",  checksum},
				{ "data", edata}
            };
			var responses = request_server(settings.authenticate, args);
			string decrypted = process_response(responses);
			return JsonConvert.DeserializeObject<Dictionary<string, object >>( decrypted );
		}



		protected Dictionary<string, object> authenticate_check(string code)
		{
			Dictionary<string, string> data = new Dictionary<string, string> {
				{ "institution", settings.institution},
				{ "notification_url", settings.notificationURL },
				{"request_datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
			};

			string str_data = JsonConvert.SerializeObject(data);
			string edata = crypto.encrypt(str_data);
			string checksum = crypto.get_hash_sum(edata, settings.algorithm);
			Dictionary<string, string> args = new Dictionary<string, string>
			{
				{ "algorithm", settings.algorithm },
				{ "public_certificate", settings.publicCertificate },
				{ "institution", settings.institution},
				{ "data_hash",  checksum},
				{ "data", edata}
			};
			string url = settings.autenticate_show.Replace("%s", code);
            var responses = request_server(url, args);
			string dataenc = System.String.Empty;
            string decrypted = process_response(responses);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
       }

		protected bool authenticate_delete(string code)
        {
            Dictionary<string, string> data = new Dictionary<string, string> {
                { "institution", settings.institution},
                { "notification_url", settings.notificationURL },
                {"request_datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            string str_data = JsonConvert.SerializeObject(data);
            string edata = crypto.encrypt(str_data);
            string checksum = crypto.get_hash_sum(edata, settings.algorithm);
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "algorithm", settings.algorithm },
                { "public_certificate", settings.publicCertificate },
                { "institution", settings.institution},
                { "data_hash",  checksum},
                { "data", edata}
            };
			string url = settings.autenticate_delete.Replace("%s", code);
            var responses = request_server(url, args);
            string decrypted = process_response(responses);
			Dictionary<string, object> ret = JsonConvert.DeserializeObject< Dictionary<string, object>>(decrypted);
			object result;
			ret.TryGetValue("result", out result);
			return (bool)result;
        }

		protected Dictionary<string, object> sign(string identification,
											byte[] document,
											string format, //xml_cofirma, xml_contrafirma, odf, msoffice
										   string resumen)
		{
			Dictionary<string, string> data = new Dictionary<string, string> {
				{ "institution", settings.institution},
				{ "notification_url", settings.notificationURL },
				{"identification", identification},
				{"request_datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
				{"format", format},
				{"resumen", resumen},
				{"document", Convert.ToBase64String(document)},
				{"algorithm_hash", settings.algorithm},
				{"document_hash", crypto.get_hash_sum(document, settings.algorithm) }
            };

            string str_data = JsonConvert.SerializeObject(data);
            string edata = crypto.encrypt(str_data);
            string checksum = crypto.get_hash_sum(edata, settings.algorithm);
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "algorithm", settings.algorithm },
                { "public_certificate", settings.publicCertificate },
                { "institution", settings.institution},
                { "data_hash",  checksum},
                { "data", edata}
            };
			var responses = request_server(settings.sign, args);
			string decrypted = process_response(responses);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
        }

        
		protected Dictionary<string, object> sign_check(string code)
        {
            Dictionary<string, string> data = new Dictionary<string, string> {
                { "institution", settings.institution},
                { "notification_url", settings.notificationURL },
                {"request_datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            string str_data = JsonConvert.SerializeObject(data);
            string edata = crypto.encrypt(str_data);
            string checksum = crypto.get_hash_sum(edata, settings.algorithm);
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "algorithm", settings.algorithm },
                { "public_certificate", settings.publicCertificate },
                { "institution", settings.institution},
                { "data_hash",  checksum},
                { "data", edata}
            };
            string url = settings.sign_show.Replace("%s", code);
            var responses = request_server(url, args);
            string decrypted = process_response(responses);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
        }


		protected bool sign_delete(string code)
        {
            Dictionary<string, string> data = new Dictionary<string, string> {
                { "institution", settings.institution},
                { "notification_url", settings.notificationURL },
                {"request_datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            string str_data = JsonConvert.SerializeObject(data);
            string edata = crypto.encrypt(str_data);
            string checksum = crypto.get_hash_sum(edata, settings.algorithm);
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "algorithm", settings.algorithm },
                { "public_certificate", settings.publicCertificate },
                { "institution", settings.institution},
                { "data_hash",  checksum},
                { "data", edata}
            };
            string url = settings.sign_delete.Replace("%s", code);
            var responses = request_server(url, args);
            string decrypted = process_response(responses);
            Dictionary<string, object> ret = JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
            object result;
            ret.TryGetValue("result", out result);
            return (bool)result;
        }

		protected Dictionary<string, object> validate(byte[] document, 
                                                      string type, 
                                                      string format = null)
		{
			Dictionary<string, string> data = new Dictionary<string, string> {
				{ "institution", settings.institution},
				{ "notification_url", settings.notificationURL },
				{"request_datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
				{"document", Convert.ToBase64String(document)}
			};

			if (format != null)
			{
				data.Add("format", format);
			}
			string str_data = JsonConvert.SerializeObject(data);
			string edata = crypto.encrypt(str_data);
			string checksum = crypto.get_hash_sum(edata, settings.algorithm);
			Dictionary<string, string> args = new Dictionary<string, string>
			{
				{ "algorithm", settings.algorithm },
				{ "public_certificate", settings.publicCertificate },
				{ "institution", settings.institution},
				{ "data_hash",  checksum},
				{ "data", edata}
			};
			string url = "";
			if (type.Equals("certificate")){
				url = settings.validate_certificate;
			}else{
				url = settings.validate_document;
			}
            var responses = request_server(url, args);
            string decrypted = process_response(responses);

			return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
		}

		protected bool suscriptor_connected(string identification){
			Dictionary<string, string> data = new Dictionary<string, string> {
                { "institution", settings.institution},
                { "notification_url", settings.notificationURL },
                {"identification", identification},
                {"request_datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            string str_data = JsonConvert.SerializeObject(data);
            string edata = crypto.encrypt(str_data);
            string checksum = crypto.get_hash_sum(edata, settings.algorithm);
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "algorithm", settings.algorithm },
                { "public_certificate", settings.publicCertificate },
                { "institution", settings.institution},
                { "data_hash",  checksum},
                { "data", edata}
            };

            var response = request_server(settings.suscriptor_conected, args);
            string result;
            response.TryGetValue("is_connected", out result);
			return Convert.ToBoolean(result);
        }

       protected Dictionary<string, object>  get_notify_data(Dictionary<string, string> data){
			string decrypted = process_response(data);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
       }
	}
    
}
