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
    public partial class ClientGame : Form
    {
        Client client;
        public ClientGame(Client client)
        {
            InitializeComponent();
            this.client = client;
            client.ClientReceive(textBox2);         
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client.ClientSend(textBox1.Text);
        }
    }
}
