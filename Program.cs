using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchTLS
{
    static class Program
    {
        internal static Config Config;

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string configPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Config.json");

            if (!File.Exists(configPath))
            {
                Config = new Config();
            }
            else
            {
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
