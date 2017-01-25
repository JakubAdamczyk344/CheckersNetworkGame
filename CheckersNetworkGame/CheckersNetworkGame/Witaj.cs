using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckersNetworkGame
{
    public partial class Witaj : Form
    {
        public Witaj()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Server server = new Server();
            this.Hide();
            server.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Client client = new Client();
            this.Hide();
            client.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GameRules gameRules = new GameRules();
            gameRules.Show();
        }
    }
}
