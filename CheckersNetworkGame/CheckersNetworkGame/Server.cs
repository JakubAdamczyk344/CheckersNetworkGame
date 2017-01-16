using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace CheckersNetworkGame
{
    public partial class Server : Form
    {
        int i;
        NetworkStream stream;
        private TcpListener server;
        private TcpClient client;
        byte[] datalength = new byte[4];

        public Server()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPAddress adresIP;
            try
            {
                adresIP = IPAddress.Parse(textBox1.Text);
            }
            catch
            {
                MessageBox.Show("Błędny format IP!", "Błąd");
                textBox1.Text = String.Empty;
                return;
            }

            int port = System.Convert.ToInt16(numericUpDown1.Value);

            try
            {
                server = new TcpListener(adresIP, port);
                server.Start();
                listBox1.Items.Add("Uruchomiono serwer, oczekuję na połączenie nowego gracza");
                button3.Enabled = true;

                client = server.AcceptTcpClient(); //Waits for the Client To Connect
                ServerGame serverGame = new ServerGame(this);
                this.Hide();
                serverGame.Show();
                //ServerReceive(); //Start Receiving
                /*new Thread(() => // Creates a New Thread (like a timer)
                {
                    client = server.AcceptTcpClient(); //Waits for the Client To Connect
                    if (client.Connected) // If you are connected
                    {
                        client = server.AcceptTcpClient(); //Waits for the Client To Connect
                        //ServerReceive(); //Start Receiving
                    }
                }).Start();*/
            }
            catch (Exception ex)
            {
                listBox1.Items.Add("Błąd inicjalizacji serwera!");
                MessageBox.Show(ex.ToString(), "Błąd");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            server.Stop();
            client.Close();
            listBox1.Items.Add("Koniec pracy");
            button1.Enabled = true;
            button3.Enabled = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Application.Exit();
        }

        public void ServerReceive(TextBox textBox)
        {
            stream = client.GetStream(); //Gets The Stream of The Connection
            new Thread(() => // Thread (like Timer)
            {
                while ((i = stream.Read(datalength, 0, 4)) != 0)//Keeps Trying to Receive the Size of the Message or Data
                {
                    // how to make a byte E.X byte[] examlpe = new byte[the size of the byte here] , i used BitConverter.ToInt32(datalength,0) cuz i received the length of the data in byte called datalength :D
                    byte[] data = new byte[BitConverter.ToInt32(datalength, 0)]; // Creates a Byte for the data to be Received On
                    stream.Read(data, 0, data.Length); //Receives The Real Data not the Size
                    this.Invoke((MethodInvoker)delegate // To Write the Received data
                    {
                        textBox.Text = System.Environment.NewLine + "Client : " + Encoding.Default.GetString(data); // Encoding.Default.GetString(data); Converts Bytes Received to String
                    });
                }
            }).Start(); // Start the Thread

        }
        public void ServerSend(string msg)
        {
            stream = client.GetStream(); //Gets The Stream of The Connection
            byte[] data; // creates a new byte without mentioning the size of it cuz its a byte used for sending
            data = Encoding.Default.GetBytes(msg); // put the msg in the byte ( it automaticly uses the size of the msg )
            int length = data.Length; // Gets the length of the byte data
            byte[] datalength = new byte[4]; // Creates a new byte with length of 4
            datalength = BitConverter.GetBytes(length); //put the length in a byte to send it
            stream.Write(datalength, 0, 4); // sends the data's length
            stream.Write(data, 0, data.Length); //Sends the real data
        }
    }
}
