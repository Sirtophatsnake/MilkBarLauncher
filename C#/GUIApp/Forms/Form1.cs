using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;

namespace GUIApp
{

    public partial class Form1 : Form
    {

        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public ServerClass Server;

        public serverList serverListForm;
        public serverInterface serverInterfaceForm;

        public Forms.DebugWindow debugWindow;

        public NamedPipes namedPipe;

        bool isHooked = false;
        public bool isHost = false;

        public discordClass discord;

        public bool isDevBuild = true;

        const string VERSION = "0.20.0";


        public Form1()
        {

            InitializeComponent();
            this.serverListForm = new serverList(this);
            this.serverInterfaceForm = new serverInterface(this);
            this.Server = new ServerClass(this);
            //this.discord = new discordClass();
            this.debugWindow = new Forms.DebugWindow(this);
            namedPipe = new NamedPipes(this);
            readXML.copyQuestFlags();
            readXML.copyWeaponDamages();
            readXML.copyServerList();
            readXML.copyArmorMappings();

            File.WriteAllText(Directory.GetCurrentDirectory() + "/version.txt", VERSION);

            string Branch = "main";

            if(File.Exists(Directory.GetCurrentDirectory() + "/Branch.txt"))
                Branch = File.ReadAllText(Directory.GetCurrentDirectory() + "/Branch.txt");

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "request");
                    var contentsUrl = $"https://api.github.com/repos/edgarcantuco/BOTW.Release/contents/version.txt?ref={Branch}";

                    var response = Task.Run(() => client.GetStringAsync(contentsUrl));
                    response.Wait();

                    var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response.Result);
                    if(VERSION != System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(json["content"].ToString())))
                    {
                        runCMD(Directory.GetCurrentDirectory() + "/BOTWMUpdater.py");
                    }
                }
            }
            catch
            {
                // Skip update check when GitHub is unavailable or rejects the request.
            }
        }

        private void runCMD(string path)
        {

            var entries = Environment.GetEnvironmentVariable("path").Split(';');
            string python_location = null;

            foreach (string entry in entries)
            {
                if (entry.ToLower().Contains("python"))
                {
                    var variables = entry.Split('\\');
                    foreach (string variable in variables)
                    {
                        if (variable.ToLower().Contains("python"))
                        {
                            python_location += variable + '\\';
                            break;
                        }
                        python_location += variable + '\\';
                    }
                    break;
                }
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = Directory.GetDirectories(python_location)[0] + "/python.exe";            
            start.Arguments = $"{Directory.GetCurrentDirectory()}/BOTWMUpdater.py";
            start.UseShellExecute = false;
            //start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                
            }
            Environment.Exit(Environment.ExitCode);

        }

        private void changeSettingsPanelState()
        {
            if (settingsPanel.Width == 250)
            {
                settingsPanel.Width = 75;
                settingsPanelStateButton.Text = "->";
            }
            else
            {
                settingsPanel.Width = 250;
                settingsPanelStateButton.Text = "<-";
            }
        }

        public void changePanel(Form FTL)
        {
            if(mainPanel.Controls.Count > 0)
                mainPanel.Controls.RemoveAt(0);

            FTL.TopLevel = false;
            FTL.Dock = DockStyle.Fill;

            mainPanel.Controls.Add(FTL);
            mainPanel.Tag = FTL;

            FTL.Show();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            serverListForm.fixServerListSize();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            changePanel(serverListForm);
            serverListForm.reloadServerList();
            //discord.update();
            //timer2.Enabled = true;
        }

        private void titleBarPanel_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void settingsPanelStateButton_Click(object sender, EventArgs e)
        {
            changeSettingsPanelState();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
            Environment.Exit(Environment.ExitCode);
        }

        private void maximizeButton_Click(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized; 
        }

        public void changeServerStatus(string Text)
        {
            serverStatusLabel.Text = Text;
        }

        public int hookToCemu()
        {
            Injector inj = new Injector(this);

            //return inj.injectDLL("Cemu", "D:\\BreathOfTheWildMultiplayer\\PythonProject\\Github\\SecretBOTWMultiplayer\\DLL\\InjectDLL\\x64\\Debug\\InjectDLL.dll");
            int injectResult = inj.injectDLL("Cemu", Directory.GetCurrentDirectory() + "\\Resources\\InjectDLL.dll");

            hookedToCemuLabel.Text = injectResult.ToString();

            return injectResult;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isHooked)
            {
                timer1.Enabled = false;
                hookedToCemuLabel.Text = "Hooked to CEMU";
            }
            else
            {
                hookedToCemuLabel.Text = "Failed to hook to CEMU. Trying again...";
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

            //discord.update();

        }

        private void button1_Click(object sender, EventArgs e)
        {

            debugWindow.Show();

        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}