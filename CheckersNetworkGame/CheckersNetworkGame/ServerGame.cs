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
            public int state;
        }

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
                    if((i%2==0 && j%2==0)||(i % 2 == 1 && j % 2 == 1))
                    {
                        board[i, j].BackColor = Color.FromArgb(206, 142, 74); //light
                    }
                    else
                    {
                        board[i, j].BackColor = Color.FromArgb(63, 61, 61); //dark
                    }
                    //setBlack(i, j);
                    board[i, j].Click += (sender1, ex) => this.ShowPosition(row, column);
                }
            }
            setPawns();
            foreach (Field b in board)
            {
                this.Controls.AddRange(new Field[] { b });
            }
        }
        public void ShowPosition(int row, int column)
        {
            textBox3.Text = "Pozycja: " + row + column;
            board[row, column].Text = "Zmieniono";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server.ServerSend(textBox1.Text);
        }

        private void setPawns()
        {
            int row = 7;
            for (int column = 0;column < 8; column+=2)
            {
                setLight(row, column);
            }
            row = 6;
            for (int column = 1; column < 8; column += 2)
            {
                setLight(row, column);
            }
            row = 5;
            for (int column = 0; column < 8; column += 2)
            {
                setLight(row, column);
            }
            row = 0;
            for (int column = 1; column < 8; column += 2)
            {
                setDark(row, column);
            }
            row = 1;
            for (int column = 0; column < 8; column += 2)
            {
                setDark(row, column);
            }
            row = 2;
            for (int column = 1; column < 8; column += 2)
            {
                setDark(row, column);
            }
        }

        private void setLight(int row, int column)
        {
            board[row, column].state = 0;
            board[row, column].BackgroundImage = Properties.Resources.LightPawn;
            board[row, column].BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void setDark(int row, int column)
        {
            board[row, column].state = 1;
            board[row, column].BackgroundImage = Properties.Resources.DarkPawn;
            board[row, column].BackgroundImageLayout = ImageLayout.Stretch;
        }
    }
}
