using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;

namespace SonarLocker
{
    public partial class frmMain : Form
    {
        private static SerialPort IO;
        static bool _continue = true;
        static int MAXDIST = 30;
        Thread readThread = null;

        public frmMain()
        {
            InitializeComponent();

            btnStop.Visible = false;
            btnStart.Visible = false;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            // get available serial ports
            if (!ListSerPorts())
            {
                MessageBox.Show("No devices found.", "Oh Monkey Pickels!!");
                Application.Exit();
            }
            else
            {
                txtTriggerDistance.Text = MAXDIST.ToString();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            initSerialPort(cmboPort.GetItemText(cmboPort.SelectedItem));

            toolStripStatusLabel1.Text = "Now sonaring through " + cmboPort.GetItemText(cmboPort.SelectedItem);

            btnStart.Visible = false;
            btnStop.Visible = true;

            MAXDIST = int.Parse(txtTriggerDistance.Text);
            txtTriggerDistance.ReadOnly = true;

            // set the max distance for the device to MAXDIST
            IO.WriteLine(MAXDIST.ToString());

            readThread = new Thread(ReadPort);
            readThread.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _continue = false;

            Application.Exit();
        }

        private void cmboPort_SelectedValueChanged(object sender, EventArgs e)
        {
            btnStart.Visible = true;
            toolStripStatusLabel1.Text = "Ready to sonar....";
        }

        public bool ListSerPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length > 0)
            {
                foreach (string port in ports)
                {
                    cmboPort.Items.Add(port);
                }

                return true;
            }
            else
            {               
                return false;
            }
        }

        private static bool initSerialPort(string serialPortNum)
        {
            try
            {
                if (IO != null)
                {
                    IO.Close();  //Just in case port is already taken
                }

                IO = new SerialPort(serialPortNum, 9600, Parity.None, 8, StopBits.One);
                IO.DtrEnable = false;
                IO.Handshake = Handshake.None;
                IO.RtsEnable = false;

                IO.Open();

                return true;
            }
            catch
            {
                serialPortNum = String.Empty;
                IO.Close();

                return false;
            }
        }

        public static void ReadPort()
        {
            while (_continue)
            {
                try
                {
                    // see if data has been sent
                    if (IO.BytesToRead == 0)
                    {
                        string message = IO.ReadLine();
                        
                        // start screen saver if TRIG is received
                        if (message.Contains("TRIG"))
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo("rundll32.exe");
                            startInfo.Arguments = "user32.dll, LockWorkStation";
                            Process.Start(startInfo);
                        }
                    }
                }
                catch (TimeoutException)
                { }
            }
        }

    }
}
