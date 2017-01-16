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
    public partial class ServerGame : Form
    {
        Server server;
        
        class Field : Button
        {
            public int positionX;
            public int positionY;
        }
        

        List<Field> buttons = new List<Field>();
        Field btn;
        Field[,] board = new Field[8,8];

        public ServerGame(Server server)
        {
            InitializeComponent();
            this.server = server;
            server.ServerReceive(textBox2);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8;j++)
                {
                    int row = i;
                    int column = j;
                    board[i, j] = new Field();
                    board[i, j].Size = new Size(50, 50);
                    board[i, j].Location = new Point((j + 1) * 50, (i + 1) * 50);
                    board[i, j].Click += (sender1, ex) => this.ShowPosition(row, column);
                }
            }
            
            foreach (Field b in board)
            {
                this.Controls.AddRange(new Field[] { b });
            }
        }
        public void ShowPosition(int row, int column)
        {
            MessageBox.Show("Pozycja: " + row + column);
            board[row, column].Text = "Zmieniono";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server.ServerSend(textBox1.Text);
        }
    }
}
