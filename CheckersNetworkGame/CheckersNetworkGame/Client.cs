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
        TcpClient client; //Definicja obiektu klasy TcpClient
        NetworkStream stream; //Utworzenie obiektu klasy NetworkStream do wysyłania i odbierania wiadomości
        byte[] datalength = new byte[4]; // creates a new byte with length 4 ( used for receivng data's lenght)

        public Client()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                string host = textBox1.Text; //pobranie adresu IP z pola tekstowego
                int port = System.Convert.ToInt16(numericUpDown1.Value); //pobranie numeru portu
                try
                {
                    client = new TcpClient(host, port); //utworzenie klienta
                    listBox1.Items.Add("Połączono z: " + host + "na porcie: " + port);
                    ClientGame clientGame = new ClientGame(this); //Otwarcie okna z grą
                    this.Hide();
                    clientGame.Show();
                }
                catch (Exception ex)
                {
                    listBox1.Items.Add("Wystąpił błąd przy próbie połączenia");
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        //zamknięcie aplikacji wraz z wywołanymi przez nią procesami po zamknięciu okna
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        //Metoda obsługująca otrzymywanie wiadomości
        public void ClientReceive(ClientGame clientGame)
        {
            //wykorzystanie strumienia danych
            stream = client.GetStream();
            new Thread(() => //utworzenie nowego wątku (by nie zawieszać działania aplikacji
            //w trakcie nasłuchiwania
            {
                while ((i = stream.Read(datalength, 0, 4)) != 0) //odczytanie danych ze strumienia, pętla kończy działanie gdy zostanie
                //osiągnięty koniec strumienia danych (funkcja zwróci 0)
                {
                    byte[] data = new byte[BitConverter.ToInt32(datalength, 0)]; //utworzenie tablicy na wiadomość o długości odebranej przed chwilą i
                    //przekonwertowanej z byte do int
                    stream.Read(data, 0, data.Length); //odczytanie wiadomości
                    this.Invoke((MethodInvoker)delegate
                    {
                        clientGame.messageFromEnemy = Encoding.Default.GetString(data); //Zwrócenie otrzymanej wiadomości do obiektu clientGame
                        clientGame.enemyMove(); //wywołanie metody obsługującej ruch przeciwnika
                    });
                }
            }).Start();
        }
        //Metoda odpowiedzialna za wysyłanie wiadomości
        public void ClientSend(string msg)
        {
            stream = client.GetStream(); //Wykorzystanie strumienia danych
            byte[] data; //utworzenie tablicy bajtów
            data = Encoding.Default.GetBytes(msg); //umieszczenie wiadomości w tablicy bajtów
            int length = data.Length; //Pobranie długości tablicy
            byte[] datalength = new byte[4]; //utworzenie tablicy bajtów
            datalength = BitConverter.GetBytes(length); //umieszczenie długości wiadomości w tablicy bajtów
            stream.Write(datalength, 0, 4); //wysłanie długości wiadomości
            stream.Write(data, 0, data.Length); //wysłanie wiadomości
        }
    }
}
