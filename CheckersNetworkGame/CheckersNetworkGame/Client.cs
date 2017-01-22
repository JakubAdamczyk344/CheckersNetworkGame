using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CheckersNetworkGame
{
    public partial class Client : Form
    {
        int i;
        TcpClient client; // Creates a TCP Client
        NetworkStream stream; //Creats a NetworkStream (used for sending and receiving data)
        byte[] datalength = new byte[4]; // creates a new byte with length 4 ( used for receivng data's lenght)

        public Client()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                string host = textBox1.Text;
                int port = System.Convert.ToInt16(numericUpDown1.Value);
                try
                {
                    client = new TcpClient(host, port);
                    listBox1.Items.Add("Połączono z: " + host + "na porcie: " + port);
                    ClientGame clientGame = new ClientGame(this);
                    this.Hide();
                    clientGame.Show();
                    //ClientReceive(); //Starts Receiving When Connected 

                }
                catch (Exception ex)
                {
                    listBox1.Items.Add("Wystąpił błąd przy próbie połączenia");
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            if (client.Connected) // if the client is connected
            {
                //ClientSend(textBox3.Text); // uses the Function ClientSend and the msg as txtSend.Text
            }
        }
        public void ClientReceive(ClientGame clientGame)
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
                        clientGame.messageFromEnemy = Encoding.Default.GetString(data); // Encoding.Default.GetString(data); Converts Bytes Received to String
                        clientGame.enemyMove();
                    });
                }
            }).Start(); // Start the Thread
        }
        public void ClientSend(string msg)
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
