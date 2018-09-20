using System;
using System.IO;
using Newtonsoft.Json;

namespace dfva_csharp.dfva
{
    public class Settings
    {
		private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public string publicCertificate = "";
		public string publicKey = "";
		public string privateKey = "";

        public string baseUrl = "";
        public string authenticate = "/authenticate/institution/";
        public string sign = "/sign/institution/";
        public string validate_certificate = "/validate/institution_certificate/";
        public string validate_document = "/validate/institution_document/";
        public string suscriptor_conected = "/validate/institution_suscriptor_connected/";
        public string autenticate_show = "/authenticate/%s/institution_show/";
        public string autenticate_delete = "/authenticate/%s/institution_delete/";
        public string sign_show = "/sign/%s/institution_show/";
        public string sign_delete = "/sign/%s/institution_delete/";
		public string institution = "";
        public string notificationURL = "N/D";
        public string algorithm = "sha512"; // sha512, sha384, sha256

        public Settings() { }


		private string get_home_folder(){

			string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX)
    ? Environment.GetEnvironmentVariable("HOME")
    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
         
			    string home = Path.Combine(homePath, ".dfva_csharp");
			if(!Directory.Exists(home)){
				Directory.CreateDirectory(home);
			}
			return home;
		}

		public bool save(){
			bool dev = true;
			string home = get_home_folder();
			string path = Path.Combine(home, "dfva_settings.json");

			try{
			    string data = JsonConvert.SerializeObject(this);

			    File.WriteAllText(path, data);
			}
            catch (Exception e)
            {
				
                log.Error(e.Message);
				dev = false;
            }
			return dev;
		}

		public bool load()
        {
			bool dev=true;
            string home = get_home_folder();
            string path = Path.Combine(home, "dfva_settings.json");

			if (File.Exists(path)){
				try{
					using (StreamReader fs = File.OpenText(path)){ 
                    Settings data = JsonConvert.DeserializeObject<Settings>(
							fs.ReadToEnd());
						this.copy(data);
					} 
				}catch (Exception e){
					log.Error(e.Message);
                    dev = false;
				}
			}else{ 
				dev = false;
				save();
			}
			if (File.Exists(publicCertificate))
            {
                using (StreamReader fs = File.OpenText(publicCertificate))
                {
                    publicCertificate = fs.ReadToEnd();
                }
            }
            return dev;
        }

		public bool copy(Settings other){
			publicCertificate = other.publicCertificate;
			publicKey = other.publicKey;
			privateKey = other.privateKey;
			baseUrl = other.baseUrl;
			authenticate = other.authenticate;
			sign = other.sign;
			validate_certificate = other.validate_certificate;
			validate_document = other.validate_document;
			suscriptor_conected = other.suscriptor_conected;
			autenticate_show = other.autenticate_show;
			autenticate_delete = other.autenticate_delete;
			sign_show = other.sign_show;
			sign_delete = other.sign_delete;
			institution = other.institution;
			notificationURL = other.notificationURL;
			algorithm = other.algorithm;
			return true;
		}
    }
}
