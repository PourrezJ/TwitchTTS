using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TwitchTTS
{
    public class Config
    {
        #region Bot
        public string Command = "tts";
        public List<string> BadWord = new List<string>
        {

        };
        public string SpeakPrefix = "dit";
        public int Delay = 0;
        #endregion

        #region Twitch
        public string LastChannel = "Twitch Channel Name";
        public string Token;
        public bool SubOnly;
        public bool ModoOnly;
        #endregion

        #region Voice
        public int VoiceRate;
        public int Volume = 80;
        public string Culture = "fr-FR";
        #endregion

        #region Methods
        public void Save()
        {
            try
            {
                var path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Config.json");
                File.Delete(path);
                using (StreamWriter outputFile = new StreamWriter(path, true))
                {
                    outputFile.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion
    }
}
