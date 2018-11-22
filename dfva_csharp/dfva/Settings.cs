using System;
using System.IO;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace dfva_csharp.dfva
{
    public class Settings
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger("dfva_csharp");

        private string _publicCertificate = null;
        public RsaKeyParameters _publicKey = null;
        public RsaPrivateCrtKeyParameters _privateKey = null;

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
        private string homePath = null;

        public Settings() { }


        private string get_home_folder()
        {

            if (this.homePath == null)
            {
                string newhomePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                       Environment.OSVersion.Platform == PlatformID.MacOSX)
        ? Environment.GetEnvironmentVariable("HOME")
        : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);


                string home = Path.Combine(newhomePath, ".dfva_csharp");
                log.Debug(home);
                if (!Directory.Exists(home))
                {
                    Directory.CreateDirectory(home);
                }
                this.homePath = home;
            }
            return this.homePath;
        }

        public void SetHomePath(string homePath) {
            this.homePath = homePath;
        }

        public RsaPrivateCrtKeyParameters get_private_key()
        {
            if (this._privateKey == null)
            {
                string key = this.privateKey.Replace("$HOME",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                );

                PemReader pr = new PemReader((StreamReader)File.OpenText(key));
                this._privateKey = (RsaPrivateCrtKeyParameters)pr.ReadObject();
            }

            return this._privateKey;
        }
        public RsaKeyParameters get_public_key()
        {
            if (this._publicKey == null)
            {
                string key = this.publicKey.Replace("$HOME",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    );
                PemReader pr = new PemReader((StreamReader)File.OpenText(key));
                this._publicKey = (RsaKeyParameters)pr.ReadObject();
            }
            return this._publicKey;
        }

        public string get_certificate()
        {
            if (this._publicCertificate == null)
            {
                string key = publicCertificate.Replace("$HOME",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                );
                if (File.Exists(key))
                {
                    using (StreamReader fs = File.OpenText(key))
                    {
                        this._publicCertificate = fs.ReadToEnd();
                    }
                }
            }
            return this._publicCertificate;
        }

        public bool save()
        {
            bool dev = true;
            string home = get_home_folder();
            string path = Path.Combine(home, "dfva_settings.json");
            string data = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, data);
            return dev;
        }

        public bool load()
        {
            bool dev = true;
            string home = get_home_folder();
            string path = Path.Combine(home, "dfva_settings.json");

            if (File.Exists(path))
            {
                using (StreamReader fs = File.OpenText(path))
                {
                    Settings data = JsonConvert.DeserializeObject<Settings>(
                            fs.ReadToEnd());
                    this.copy(data);
                }
            }
            else
            {
                dev = false;
                save();
            }
            return dev;
        }

        public bool copy(Settings other)
        {
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
