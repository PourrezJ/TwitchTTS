using System;
using System.Windows.Forms;

namespace TwitchTTS
{
    public partial class Main : Form
    {
        private delegate void SafeCallDelegate(string text);
        private static TwitchConnect twitchConnect;

        public static Main Instance;

        public Main()
        {
            Instance = this;

            InitializeComponent();

            TextBoxChangeText(Program.Config.LastChannel);
            checkboxsub.Checked = Program.Config.SubOnly;
            numericUpDown1.Value = Program.Config.VoiceRate;
            numericUpDown2.Value = Program.Config.Delay;
            modoOnlyCheck.Checked = Program.Config.ModoOnly;
            numericUpDown3.Value = Program.Config.Volume;
        }

        private void textBoxChannelName(object sender, EventArgs e)
        {
            
        }

        private void connectionButton(object sender, EventArgs e)
        {
            if (twitchConnect == null)
                twitchConnect = new TwitchConnect(textBox1.Text.ToLower());
            else
            {
                twitchConnect.Dispose();
                twitchConnect = null;
                ButtonChangeText("Disconnected");
            }
        }

        public void ButtonChangeText(string text)
        {
            if (button1.InvokeRequired)
            {
                var d = new SafeCallDelegate(ButtonChangeText);
                button1.Invoke(d, new object[] { text });
            }
            else
            {
                button1.Text = text;
            }
        }

        public void TextBoxChangeText(string text)
        {
            if (textBox1.InvokeRequired)
            {
                var d = new SafeCallDelegate(ButtonChangeText);
                textBox1.Invoke(d, new object[] { text });
            }
            else
            {
                textBox1.Text = text;
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Program.Config.VoiceRate = (int)numericUpDown1.Value;
            Program.Config.Save();

            if (TwitchConnect.Synthesizer != null)
            {
                TwitchConnect.Synthesizer.Rate = Program.Config.VoiceRate;
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Program.Config.Delay = (int)numericUpDown2.Value;
            Program.Config.Save();
        }

        private void credit_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/PourrezJ");
        }

        private void checkboxsub_CheckedChanged(object sender, EventArgs e)
        {
            Program.Config.SubOnly = checkboxsub.Checked;
            Program.Config.Save();
        }

        private void modoOnlyCheck_CheckedChanged(object sender, EventArgs e)
        {
            Program.Config.ModoOnly = modoOnlyCheck.Checked;
            Program.Config.Save();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Program.Config.Volume = (int)numericUpDown3.Value;
            Program.Config.Save(); 
            if (TwitchConnect.Synthesizer != null)
                TwitchConnect.Synthesizer.Volume = (int)numericUpDown3.Value;
        }
    }
}
