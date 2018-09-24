using System;
using System.Collections.Generic;

namespace dfva_csharp.dfva
{
public class Client: InternalClient
	{
        private static readonly log4net.ILog log =
			                       log4net.LogManager.GetLogger("dfva_csharp");

		public Client(Settings dfvasettings): base(dfvasettings)
		{
			
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

		new public Dictionary<string, object> authenticate(string identification){
			Dictionary<string, object> dev;
			try{
				dev = base.authenticate(identification);
			}catch (Exception e) {
				dev = get_default_sign_error();
				log.Error("dfva_csharp authenticate: " +e.Message);
			}
            return dev;
		}

		new public Dictionary<string, object> authenticate_check(string code)
		{
			Dictionary<string, object> dev;
            try {
				dev = base.authenticate_check(code);
			} catch (Exception e)  {
                dev = get_default_sign_error();
				log.Error("dfva_csharp authenticate_check: " +e.Message);
            }
            return dev;
		}

		new public bool authenticate_delete(string code)
        {
			bool dev = false;
            try
            {
				dev = base.authenticate_delete(code);
            }
			catch (Exception e)
            {
                dev = false;
				log.Error("dfva_csharp authenticate_delete: " +e.Message);
            }
            return dev;
        }

		new public Dictionary<string, object> sign(string identification,
                                            byte[] document,
                                            string format, //xml_cofirma, xml_contrafirma, odf, msoffice
                                           string resumen)
		{
			Dictionary<string, object> dev;
            try
            {
                dev = base.sign(identification, 
				                 document,
				                 format,
				                 resumen );
            }
			catch (Exception e)
            {
                dev = get_default_sign_error();
				log.Error("dfva_csharp sign: " + e.Message);
            }
            return dev;
		}
        new public Dictionary<string, object> sign_check(string code)
        {
            Dictionary<string, object> dev;
            try
            {
                dev = base.sign_check(code);
            }
			catch (Exception e)
            {
                dev = get_default_sign_error();
				log.Error("dfva_csharp sign_check: " +e.Message);
            }
            return dev;
        }

        new public bool sign_delete(string code)
        {
            bool dev = false;
            try
            {
                dev = base.sign_delete(code);
            }
			catch (Exception e)
            {
                dev = false;
				log.Error("dfva_csharp sign_delete: " +e.Message);
            }
            return dev;
        }
		new public Dictionary<string, object> validate(byte[] document, 
                                                   string type, 
                                                   string format = null){
			Dictionary<string, object> dev;
            try
            {
				dev = base.validate(document, type, format);
            }
			catch (Exception e)
            {
                dev = get_default_validate_error();
				log.Error("dfva_csharp validate: "+e.Message);
            }
            return dev;			
		}        

		new public bool suscriptor_connected(string identification)
        {
            bool dev = false;
            try
            {
				dev = base.suscriptor_connected(identification);
            }
            catch (Exception e)
			{
                dev = false;
				log.Error("dfva_csharp suscriptor_connected: " +e.Message);
            }
            return dev;
        }

        new public Dictionary<string, object>  get_notify_data(Dictionary<string, string> data){
            Dictionary<string, object>  dev=null;
            try
            {
				dev = base.get_notify_data(data);
            }catch (Exception e)
			{
				log.Error("dfva_csharp get_notify_data: "+e.Message);
            }
            return dev;
        }

    }
}