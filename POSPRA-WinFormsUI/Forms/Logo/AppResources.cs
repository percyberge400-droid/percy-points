using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace POSPRA_WinFormsUI.Forms.Logo
{
    internal static class AppResources
    {
        private static Image _cachedLogo;

        public static Image BusinessLogo
        {
            get
            {
                if (_cachedLogo != null)
                    return _cachedLogo;

                string file = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "POSPRA",
                    "AppSettings.config"
                );

                if (!File.Exists(file))
                    return null;

                var xml = new System.Xml.XmlDocument();
                xml.Load(file);
                var node = xml.SelectSingleNode("//add[@key='CompLogobase64']") as System.Xml.XmlElement;
                if (node == null) return null;

                string base64 = node.GetAttribute("value");
                if (string.IsNullOrEmpty(base64)) return null;

                byte[] bytes = Convert.FromBase64String(base64);
                using var ms = new MemoryStream(bytes);
                _cachedLogo = Image.FromStream(ms);
                return _cachedLogo;
            }
        }
    }
}
