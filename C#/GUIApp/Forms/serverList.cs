using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIApp
{
    public partial class serverList : Form
    {

        public Dictionary<string, string>[] serverListDict;
        Bitmap[] placeholderImages = { Properties.Resources.Mipha2, Properties.Resources.Revali, Properties.Resources.Urbosa, Properties.Resources.Daruk };
        public Form1 mainWindow;

        public serverList(Form1 mW)
        {
            InitializeComponent();
            this.mainWindow = mW;
            this.NameTextbox.Text = Properties.Settings.Default.playerName;
        }

        public void reloadServerList()
        {
            fixServerListSize();

            serverListDict = readXML.getServerList(2) ?? new Dictionary<string, string>[0];
            int count = 0;

            for (int i = serverPanel.Controls.Count - 1; i > -1; i--)
            {
                serverPanel.Controls.Remove(serverPanel.Controls[i]);
            }

            int panelNumber = 0;

            foreach (Dictionary<string, string> item in serverListDict)
            {
                Panel newPanel = new Panel();
                newPanel.Size = new Size(serverPanel.Width - 6, 100);
                newPanel.Name = "serverPanel_" + panelNumber.ToString();

                Bitmap placeholderImage = GetPlaceholderImage(count);
                PictureBox serverPicture = addControls.createPB(placeholderImage, 5, 5, 90, 90);
                Label nameLabel1 = addControls.createLabel("Server name:", 103, 5);
                Label nameLabel2 = addControls.createLabel(item.ContainsKey("Name") ? item["Name"] : "Placeholder Name", 212, 5);
                Label descriptionLabel1 = addControls.createLabel("Description:", 103, 25);
                Label descriptionLabel2 = addControls.createLabel(item.ContainsKey("DESCRIPTION") ? item["DESCRIPTION"] : "Placeholder description", 212, 25);
                Label settingsLabel = addControls.createLabel("Settings:", 103, 45);
                Button connectButton = addControls.createButton("Connect", newPanel.Width - 103, 3, 100, 28);
                Button editButton = addControls.createButton("Edit", newPanel.Width - 103, 36, 100, 28);
                Button removeButton = addControls.createButton("Remove", newPanel.Width - 103, 69, 100, 28);

                connectButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                editButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                removeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

                Control[] panelControls =
                {
                    serverPicture,
                    nameLabel1,
                    nameLabel2,
                    descriptionLabel1,
                    descriptionLabel2,
                    settingsLabel,
                    connectButton,
                    editButton,
                    removeButton
                };

                foreach (Control ctrl in panelControls)
                {
                    newPanel.Controls.Add(ctrl);
                    ctrl.MouseEnter += new EventHandler(this.mouseInPanel);
                }

                newPanel.MouseEnter += new EventHandler(this.mouseInPanel);
                newPanel.MouseLeave += new EventHandler(this.mouseOutPanel);

                connectButton.Click += new EventHandler(this.connectClick);

                serverPanel.Controls.Add(newPanel);
                serverPanel.SetFlowBreak(newPanel, true);

                Color newColor;

                if (Int32.Parse(newPanel.Name.Substring(12)) % 2 == 0)
                {
                    newColor = Color.FromKnownColor(KnownColor.Control);
                }
                else
                {
                    newColor = Color.FromKnownColor(KnownColor.ControlLightLight);
                }

                newPanel.BackColor = newColor;
                foreach (Control childCtrl in newPanel.Controls)
                {
                    if (childCtrl.GetType() != typeof(Button))
                    {
                        childCtrl.BackColor = newColor;
                    }else
                    {
                        childCtrl.BackColor = Color.FromKnownColor(KnownColor.Control);
                    }
                }

                count++;
                if (count == 4)
                {
                    count = 0;
                }

                panelNumber++;
            }
        }

        private Bitmap GetPlaceholderImage(int index)
        {
            if (placeholderImages == null || placeholderImages.Length == 0)
                return new Bitmap(90, 90);

            Bitmap image = placeholderImages[index % placeholderImages.Length];
            return image ?? new Bitmap(90, 90);
        }

        private void connectClick(object sender, EventArgs e)
        {

            Button ctrl = (Button)sender;
            int ID = Int32.Parse(ctrl.Parent.Name.Substring(12));
            string[] serverData = { "Testing", "Testing2", serverListDict[ID]["PASSWORD"], serverListDict[ID]["IP"], serverListDict[ID]["PORT"] };

            if (!connectToServer(serverData, true))
            {
                return;
            }

        }

        private void mouseInPanel(object sender, EventArgs e)
        {
            Control snd = (Control)sender;
            Control ctrl;
            Color newColor;

            if (snd.Parent.GetType() == typeof(Panel))
            {
                ctrl = snd.Parent;
            }
            else
            {
                ctrl = snd;
            }

            if (Int32.Parse(ctrl.Name.Substring(12)) % 2 == 0)
            {
                newColor = Color.FromKnownColor(KnownColor.ControlLight);
            }
            else
            {
                newColor = Color.FromKnownColor(KnownColor.ControlLight);
            }

            ctrl.BackColor = newColor;
            foreach (Control childCtrl in ctrl.Controls)
            {
                if (childCtrl.GetType() != typeof(Button))
                {
                    childCtrl.BackColor = newColor;
                }
            }
        }

        private void mouseOutPanel(object sender, EventArgs e)
        {
            Control snd = (Control)sender;
            Control ctrl;
            Color newColor;

            if (snd.Parent.GetType() == typeof(Panel))
            {
                ctrl = snd.Parent;
            }
            else
            {
                ctrl = snd;
            }

            if (Int32.Parse(ctrl.Name.Substring(12)) % 2 == 0)
            {
                newColor = Color.FromKnownColor(KnownColor.Control);
            }
            else
            {
                newColor = Color.FromKnownColor(KnownColor.ControlLightLight);
            }

            ctrl.BackColor = newColor;
            foreach (Control childCtrl in ctrl.Controls)
            {
                if (childCtrl.GetType() != typeof(Button))
                {
                    childCtrl.BackColor = newColor;
                }
            }
        }

        public void fixServerListSize()
        {
            foreach (Control ctrl in serverPanel.Controls)
            {
                ctrl.Width = serverPanel.Width - 6;
            }
        }

        private void hostButton_Click(object sender, EventArgs e)
        {

            HostServer hostServerWindow = new HostServer(mainWindow);
            if(hostServerWindow.ShowDialog() == DialogResult.OK)
            {

                mainWindow.Server.startListen();
                hostServerWindow.results[3] = mainWindow.Server.IP;

                if(!connectToServer(hostServerWindow.results, true))
                {
                    mainWindow.Server.stopServer();
                }

            }

        }

        private void addButton_Click(object sender, EventArgs e)
        {

            addServer addServerWindow = new addServer();

            if(addServerWindow.ShowDialog() == DialogResult.OK)
            {

                readXML.addServerToList(serverListDict, addServerWindow.results[0], addServerWindow.results[1], addServerWindow.results[2]);
                reloadServerList();

            }

        }

        private void directConnectionButton_Click(object sender, EventArgs e)
        {

            

        }

        private bool connectToServer(string[] serverData, bool isHost)
        {

            if (new messageBoxUnclosable(mainWindow, serverData[3], serverData[4], serverData[2]).ShowDialog() == DialogResult.OK)
            {
                mainWindow.isHost = isHost;
                mainWindow.serverInterfaceForm.jugador1_nameLabel.Text = Properties.Settings.Default.playerName + " (HOST)";
                mainWindow.serverInterfaceForm.jugador1_nameLabel.ForeColor = Color.Black;
                mainWindow.serverInterfaceForm.serverName = serverData[0];
                mainWindow.serverInterfaceForm.serverDescription = serverData[1];
                mainWindow.changePanel(mainWindow.serverInterfaceForm);
                return true;
            }

            return false;

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void filterPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void NameTextbox_TextChanged(object sender, EventArgs e)
        {

            Properties.Settings.Default.playerName = NameTextbox.Text;
            Properties.Settings.Default.Save();

        }
    }
}