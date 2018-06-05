using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;

namespace dfva_csharp.Properties
{
	public class Client
	{
		private Settings settings;
		private Crypto crypto;
		private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public Client(Settings dfvasettings)
		{
			settings = dfvasettings;
			crypto = new Crypto(dfvasettings);
		}

		private Dictionary<string, object> get_default_sign_error()
		{
			Dictionary<string, object> dev = new Dictionary<string, object> {
				{"code", "N/D"},
				{"status",  2},
				{"identification", null},
				{"id_transaction", 0},
				{"request_datetime", ""},
				{"sign_document", ""},
				{"expiration_datetime", ""},
                {"received_notification", true},
                { "duration", 0},
                {"status_text", "Problema de comunicación interna"}
			};
         
			return dev;
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
        
		public Dictionary<string, object> _authenticate(string identification)
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
			var response = request_server(settings.authenticate, args);
            string dataenc = System.String.Empty;
            response.TryGetValue("data", out dataenc);
			string decrypted = crypto.decrypt(dataenc);

			return JsonConvert.DeserializeObject<Dictionary<string, object >>( decrypted );
		}
		public Dictionary<string, object> authenticate(string identification){
			Dictionary<string, object> dev;
			try{
				dev = this._authenticate(identification);
			}catch (Exception e) {
				dev = get_default_sign_error();
				log.Error(e.Message);
			}
            return dev;
		}


		private Dictionary<string, object> _authenticate_check(string code)
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
            responses.TryGetValue("data", out dataenc);
            string decrypted = crypto.decrypt(dataenc);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
       }
		public Dictionary<string, object> authenticate_check(string code)
		{
			Dictionary<string, object> dev;
            try {
				dev = this._authenticate_check(code);
			} catch (Exception e)  {
                dev = get_default_sign_error();
				log.Error(e.Message);
            }
            return dev;
		}
		private bool _authenticate_delete(string code)
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
            string dataenc = System.String.Empty;
            responses.TryGetValue("data", out dataenc);
            string decrypted = crypto.decrypt(dataenc);
			Dictionary<string, object> ret = JsonConvert.DeserializeObject< Dictionary<string, object>>(decrypted);
			object result;
			ret.TryGetValue("result", out result);
			return (bool)result;
        }
		public bool authenticate_delete(string code)
        {
			bool dev = false;
            try
            {
				dev = this._authenticate_delete(code);
            }
			catch (Exception e)
            {
                dev = false;
				log.Error(e.Message);
            }
            return dev;
        }

		private Dictionary<string, object> _sign(string identification,
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
			var response = request_server(settings.sign, args);
            string dataenc = System.String.Empty;
            response.TryGetValue("data", out dataenc);
            string decrypted = crypto.decrypt(dataenc);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
        }

		public Dictionary<string, object> sign(string identification,
                                            byte[] document,
                                            string format, //xml_cofirma, xml_contrafirma, odf, msoffice
                                           string resumen)
		{
			Dictionary<string, object> dev;
            try
            {
                dev = this._sign(identification, 
				                 document,
				                 format,
				                 resumen );
            }
			catch (Exception e)
            {
                dev = get_default_sign_error();
				log.Error(e.Message);
            }
            return dev;
		}
		private Dictionary<string, object> _sign_check(string code)
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
            string dataenc = System.String.Empty;
            responses.TryGetValue("data", out dataenc);
            string decrypted = crypto.decrypt(dataenc);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
        }
        public Dictionary<string, object> sign_check(string code)
        {
            Dictionary<string, object> dev;
            try
            {
                dev = this._sign_check(code);
            }
			catch (Exception e)
            {
                dev = get_default_sign_error();
				log.Error(e.Message);
            }
            return dev;
        }

		private bool _sign_delete(string code)
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
            string dataenc = System.String.Empty;
            responses.TryGetValue("data", out dataenc);
            string decrypted = crypto.decrypt(dataenc);
            Dictionary<string, object> ret = JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
            object result;
            ret.TryGetValue("result", out result);
            return (bool)result;
        }
        public bool sign_delete(string code)
        {
            bool dev = false;
            try
            {
                dev = this._sign_delete(code);
            }
			catch (Exception e)
            {
                dev = false;
				log.Error(e.Message);
            }
            return dev;
        }

		private Dictionary<string, object> _validate(byte[] document, string type, string format = null)
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
            string dataenc = System.String.Empty;
            responses.TryGetValue("data", out dataenc);
            string decrypted = crypto.decrypt(dataenc);

			return JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
		}

		public Dictionary<string, object> validate(byte[] document, string type, string format = null){
			Dictionary<string, object> dev;
            try
            {
				dev = this._validate(document, type, format);
            }
			catch (Exception e)
            {
                dev = get_default_validate_error();
				log.Error(e.Message);
            }
            return dev;			
		}

		private Dictionary<string, object> get_default_validate_error()
		{
			Dictionary<string, object> dev = new Dictionary<string, object> {
                {"code", "N/D"},
                {"status",  2},
                {"identification", null},
                {"id_transaction", 0},
                {"sign_document", ""},
                {"expiration_datetime", ""},
                {"received_notification", true},
                {"status_text", "Problema de comunicación interna"}
            };

            return dev;

		}

		private bool _suscriptor_connected(string identification){
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
		public bool suscriptor_connected(string identification)
        {
            bool dev = false;
            try
            {
				dev = this._suscriptor_connected(identification);
            }
            catch (Exception e)
			{
                dev = false;
				log.Error(e.Message);
            }
            return dev;
        }

	}
    
}
