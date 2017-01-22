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
        NetworkStream stream; //Utworzenie obiektu klasy NetworkStream do wysyłania i odbierania wiadomości
        private TcpListener server; //Definicja obiektu klasy TcpListener
        private TcpClient client; //Definicja obiektu klasy TcpClient
        byte[] datalength = new byte[4];

        public Server()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPAddress adresIP; //Definicja obiektu klasy IPAdress
            try
            {
                adresIP = IPAddress.Parse(textBox1.Text); //Konwersja tekstu z pola tekstowego do adresu IP
            }
            catch
            {
                MessageBox.Show("Błędny format IP!", "Błąd");
                textBox1.Text = String.Empty;
                return;
            }

            int port = System.Convert.ToInt16(numericUpDown1.Value); //pobranie numeru portu

            try
            {
                server = new TcpListener(adresIP, port); //utworzenie obiektu klasy TcpListener
                server.Start(); //rozpoczęcie nasłuchiwania
                listBox1.Items.Add("Uruchomiono serwer, oczekuję na połączenie nowego gracza");
                button3.Enabled = true;

                client = server.AcceptTcpClient(); //Waits for the Client To Connect
                ServerGame serverGame = new ServerGame(this); //Otwarcie okna z grą
                this.Hide();
                serverGame.Show();
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
        //zamknięcie aplikacji wraz z wywołanymi przez nią procesami po zamknięciu okna
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        //Metoda obsługująca otrzymywanie wiadomości
        public void ServerReceive(ServerGame serverGame)
        {
            stream = client.GetStream(); //wykorzystanie strumienia danych
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
                        serverGame.messageFromEnemy =  Encoding.Default.GetString(data); //Zwrócenie otrzymanej wiadomości do obiektu serverGame
                        serverGame.enemyMove(); //wywołanie metody obsługującej ruch przeciwnika
                    });
                }
            }).Start();

        }
        //Metoda odpowiedzialna za wysyłanie wiadomości
        public void ServerSend(string msg)
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
