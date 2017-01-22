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
        //definicja obiektu server, którego metody serverSend i serverReceive zostaną wykorzystane
        Server server;
        //Plansza do gry składa się z przycisków, posiadających dodatkowo jedną właściwość:
        //state, która mówi o tym jaka figura znajduje się na danym polu
        class Field : Button
        {
            public string state;
        }

        int numberOfLightPawns; //liczba białych pionków
        int numberOfDarkPawns; //liczba ciemnych pionków
        int click=1; //licznik kliknięć, wykorzystywany do wyboru pionka do ruchu (1 klik) i gdzie
        //go ruszyć (2 klik)
        int whichMessageFromEnemy = 1; //licznik wiadomości od przeciwnika (1 to informacja skąd i dokąd ruch,
        //2 to informacja, czy tura się zakończyła czy będzie kolejne bicie)
        int whichPawnMoveRow; //z którego rzędu pionek będzie wykonywał ruch
        int whichPawnMoveColumn; //z której kolumny pionek będzie wykonywał ruch
        int wherePawnMoveRow; //na który rząd pionek wykona ruch
        int wherePawnMoveColumn; //na którą kolumnę pionek wykona ruch
        bool isMoveLegal; //czy ruch jest możliwy
        bool hasToGrab; //czy jest możliwość bicia
        string isPawnLost; //czy przeciwnik zbił nam pionka
        string isMoveOver; //czy ruch został zakończony
        public string messageFromEnemy; //wiadomość o ruchu przeciwnika
        string whoseTurn; //określenie, czyja jest teraz tura

        Field[,] board = new Field[8,8]; //utworzenie zalążka planszy, czyli tablicy pól 8x8

        public ServerGame(Server server) //konstruktor obieku klasy ServerGame, wywołany
            //w momencie rozpoczęcia gry
        {
            InitializeComponent();
            this.server = server;
            server.ServerReceive(this); //rozpoczęcie nasłuchiwania wiadomości od przeciwnika
            whoseTurn = "light"; //określenie czyja jest tura
            numberOfLightPawns = 12; //ustawienie licznika pionków (zmniejszanego po każdym zbiciu)
            numberOfDarkPawns = 12;
            for (int i = 0; i < 8; i++) //utworzenie planszy do gry
            {
                for (int j = 0; j < 8;j++)
                {
                    int row = i;
                    int column = j;
                    board[i, j] = new Field(); //ustalenie rozmiaru pola i jego lokalizacji
                    board[i, j].Size = new Size(50, 50);
                    board[i, j].Location = new Point((j + 1) * 50, (i + 1) * 50);
                    board[i, j].state = "empty"; //ustalenie stanu pola (na tym etapie wszystkie są puste)
                    board[i, j].FlatStyle = FlatStyle.Flat; //zmiana stylu wizualnego przycisków
                    if ((i%2==0 && j%2==0)||(i % 2 == 1 && j % 2 == 1)) //ustawienie kolorów pól: ciemnego i jasnego
                    {
                        board[i, j].BackgroundImage = Properties.Resources.emptyLight;
                    }
                    else
                    {
                        board[i, j].BackgroundImage = Properties.Resources.emptyDark;
                    }
                    //metoda obsługująca kliknięcie na pole, wywołanie funkcji rusz pionek z przesłaniem
                    //do tej funkcji jego pozycji
                    board[i, j].Click += (sender1, ex) => this.movePawn(row, column);
                }
            }
            setPawns(); //rozstawienie pionków jasnych i ciemnych na planszy
            hasToGrab = false;
            foreach (Field b in board) //wyświetlenie planszy na ekranie
            {
                this.Controls.AddRange(new Field[] { b });
            }
        }
        public void movePawn(int row, int column) //funcja odpowiedzialna za przemieszczanie pionków
        {
            if (whoseTurn == "light") //sprawdzenie, czy gracz ma prawo ruchu
            {
                textBox3.Text = "Pozycja: " + row + column;
                //pierwsze kliknięcie, jeśli gracz klika na swój pionek lub damkę to dodać zieloną ramkę
                //i zapisać współrzędne pionka, który gracz ma zamiar poruszyć
                if ((click == 1) && ((board[row, column].state == "lightPawn") || (board[row, column].state == "lightKing")))
                {
                    board[row, column].FlatAppearance.BorderColor = Color.Green;
                    board[row, column].FlatAppearance.BorderSize = 3;
                    whichPawnMoveRow = row;
                    whichPawnMoveColumn = column;
                    click++;
                    return;
                }
                //drugie kliknięcie, jeśli kliknięte pole jest puste to zapisać jego współrzędne
                if ((click == 2) && (board[row, column].state == "empty"))
                {
                    wherePawnMoveRow = row;
                    wherePawnMoveColumn = column;
                    checkMovePropriety(); //sprawdzić możliwość wykonania ruchu
                    if (isMoveLegal == true)
                    {
                        click = 1;
                        //ustawić na wybranym polu damkę
                        if (board[whichPawnMoveRow, whichPawnMoveColumn].state == "lightKing")
                        {
                            setLightKing(wherePawnMoveRow, wherePawnMoveColumn);
                        }
                        //jeśli pionek dochodzi do końca planszy to zmienić go w damkę
                        else if (wherePawnMoveRow == 0)
                        {
                            setLightKing(wherePawnMoveRow, wherePawnMoveColumn);
                        }
                        //ustawić na wybranym polu pionek
                        else
                        {
                            setLight(wherePawnMoveRow, wherePawnMoveColumn);
                        }
                        //opróżnić pole, z którego dokonał się ruch
                        setEmpty(whichPawnMoveRow, whichPawnMoveColumn);
                        if (hasToGrab==true) //jeśli jest możliwość bicia to wykonać je
                        {
                            doGrab();
                            numberOfDarkPawns--; //zmniejszyć licznik pionków przeciwnika
                        }
                        //usunąć zieloną obwódkę z poruszanego pionka
                        board[whichPawnMoveRow, whichPawnMoveColumn].FlatAppearance.BorderColor = Color.Black;
                        board[whichPawnMoveRow, whichPawnMoveColumn].FlatAppearance.BorderSize = 1;
                        server.ServerSend(whichPawnMoveRow.ToString() + whichPawnMoveColumn.ToString() + wherePawnMoveRow.ToString() + wherePawnMoveColumn.ToString() + hasToGrab);
                        if (hasToGrab == true) //jeśli poprzednio było bicie, to sprawdzić, czy można wykonać kolejne
                        {
                            isGrabPossible();
                            if (hasToGrab == true)
                            {
                                whoseTurn = "light"; //jeśli można dalej bić to kontynuować obecną turę
                                whoseTurnLabel.Text = "Twój ruch";
                                isMoveOver = "N";
                            }
                            else
                            {
                                whoseTurn = "dark"; //jeślie nie ma kolejnego bicia to nowa tura
                                whoseTurnLabel.Text = "Czekaj na ruch przeciwnika";
                                isMoveOver = "Y";
                            }
                        }
                        else //jeśli nie było żadnego bicia to przejść do kolejnej tury
                        {
                            whoseTurn = "dark";
                            whoseTurnLabel.Text = "Czekaj na ruch przeciwnika";
                            isMoveOver = "Y";
                        }
                        server.ServerSend(isMoveOver); //wysłać do przeciwnika wiadomość mówiącą czy to koniec tury
                        checkIfGameIsOver(); //sprawdzić, czy gra została zakończona
                    }
                }
                //jeśli gracz wybrał pionek do ruchu, ale się rozmyślił i wybrał inny to usunąć zieloną
                //ramkę wokół poprzedniego i dodać do nowego
                if ((click == 2) && ((board[row, column].state == "lightPawn") || (board[row, column].state == "lightKing")))
                {
                    board[row, column].FlatAppearance.BorderColor = Color.Green;
                    board[row, column].FlatAppearance.BorderSize = 3;
                    board[whichPawnMoveRow, whichPawnMoveColumn].FlatAppearance.BorderColor = Color.Black;
                    board[whichPawnMoveRow, whichPawnMoveColumn].FlatAppearance.BorderSize = 1;
                    whichPawnMoveRow = row;
                    whichPawnMoveColumn = column;
                }
            }
        }
        //metoda, która obsługuje ruch przeciwnika
        public void enemyMove()
        {
            //jeśli otrzymujemy pierwszą wiadomość od przeciwnika to pobrać informacje o jego ruchu (skąd i dokąd
            //i czy dochodzi do bicia)
            if (whichMessageFromEnemy == 1)
            {
                textBox3.Text = messageFromEnemy.Substring(4, 1);
                whichPawnMoveRow = Convert.ToInt16(messageFromEnemy.Substring(0, 1));
                whichPawnMoveColumn = Convert.ToInt16(messageFromEnemy.Substring(1, 1));
                wherePawnMoveRow = Convert.ToInt16(messageFromEnemy.Substring(2, 1));
                wherePawnMoveColumn = Convert.ToInt16(messageFromEnemy.Substring(3, 1));
                isPawnLost = messageFromEnemy.Substring(4, 1);
                //ustawić damkę przeciwnika na wskazanym polu
                if (board[whichPawnMoveRow, whichPawnMoveColumn].state == "darkKing")
                {
                    setDarkKing(wherePawnMoveRow, wherePawnMoveColumn);
                }
                //zamienić pionek przeciwnika na damkę, jeśli dochodzi on do dołu planszy
                else if (wherePawnMoveRow == 7)
                {
                    setDarkKing(wherePawnMoveRow, wherePawnMoveColumn);
                }
                //ustawić pionek przeciwnika na wskazanym polu
                else
                {
                    setDark(wherePawnMoveRow, wherePawnMoveColumn);
                }
                //opróżnić pole zajmowane wcześniej przez przeciwnika
                setEmpty(whichPawnMoveRow, whichPawnMoveColumn);
                if (isPawnLost == "T") //wykonać bicie, jeśli do niego dochodzi
                {
                    doGrab();
                    numberOfLightPawns--; //zmniejszyć licznik naszych pionków
                }
                whichMessageFromEnemy = 2; //ustalić, że czekamy na drugą wiadomość
                return;
            }
            if (whichMessageFromEnemy == 2) //odbiór drugiej wiadomości (czy koniec tury czy nie)
            {
                isMoveOver = messageFromEnemy.Substring(0, 1);
                if (isMoveOver=="N")
                {
                    whoseTurn = "dark";
                    whoseTurnLabel.Text = "Czekaj na ruch przeciwnika";
                }
                if (isMoveOver == "Y")
                {
                    whoseTurn = "light";
                    whoseTurnLabel.Text = "Twój ruch";
                    isGrabPossible(); //z rozpoczęciem naszej tury sprawdzamy, czy mamy możliwość bicia
                }
                checkIfGameIsOver(); //sprawdzamy, czy gra się zakończyła
                whichMessageFromEnemy = 1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server.ServerSend(textBox1.Text);
        }

        private void setPawns() //początkowe rozstawienie pionków
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

        private void setLight(int row, int column) //metoda ustwiająca jasny pionek na wybranym polu
        {
            board[row, column].state = "lightPawn";
            board[row, column].BackgroundImage = Properties.Resources.LightPawn;
            board[row, column].BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void setDark(int row, int column) //metoda ustwiająca ciemny pionek na wybranym polu
        {
            board[row, column].state = "darkPawn";
            board[row, column].BackgroundImage = Properties.Resources.DarkPawn;
            board[row, column].BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void setLightKing(int row, int column) //metoda ustwiająca jasną damkę na wybranym polu
        {
            board[row, column].state = "lightKing";
            board[row, column].BackgroundImage = Properties.Resources.LightKing;
            board[row, column].BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void setDarkKing(int row, int column) //metoda ustwiająca ciemną damkę na wybranym polu
        {
            board[row, column].state = "darkKing";
            board[row, column].BackgroundImage = Properties.Resources.DarkKing;
            board[row, column].BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void setEmpty(int row, int column)
        {
            board[row, column].state = "empty"; //metoda opróżniająca wybrne pole
            board[row, column].BackgroundImage = Properties.Resources.emptyDark;
        }

        private void checkMovePropriety() //metoda sprawdzająca, czy ruch jest możliwy, wzięte jest pod uwagę, że
            //damka może poruszać się i bić do tyłu i że możliwość bicia sprawia, że konieczne jest przeskoczenie
            //przeciwnika
        {

            if (((wherePawnMoveRow == whichPawnMoveRow - 1) && ((wherePawnMoveColumn == whichPawnMoveColumn - 1) || (wherePawnMoveColumn == whichPawnMoveColumn + 1))) && hasToGrab == false)
            {
                isMoveLegal = true;
            }
            else if (board[whichPawnMoveRow,whichPawnMoveColumn].state == "lightKing" && ((wherePawnMoveRow == whichPawnMoveRow + 1) && ((wherePawnMoveColumn == whichPawnMoveColumn - 1) || (wherePawnMoveColumn == whichPawnMoveColumn + 1))) && hasToGrab == false)
            {
                isMoveLegal = true;
            }
            else if (((wherePawnMoveRow == whichPawnMoveRow - 2) && ((wherePawnMoveColumn == whichPawnMoveColumn - 2) || (wherePawnMoveColumn == whichPawnMoveColumn + 2))) && hasToGrab == true)
            {
                isMoveLegal = true;
            }
            else if (board[whichPawnMoveRow, whichPawnMoveColumn].state == "lightKing" && ((wherePawnMoveRow == whichPawnMoveRow + 2) && ((wherePawnMoveColumn == whichPawnMoveColumn - 2) || (wherePawnMoveColumn == whichPawnMoveColumn + 2))) && hasToGrab == true)
            {
                isMoveLegal = true;
            }
            else
            {
                isMoveLegal = false;
            }
        }
        //sprawdzenie czy jest możliwość bicia, metoda jest rozbita na kilka instrukcji warunkowych, bo w zależności
        //od tego czy mamy do czynienia z damką czy pionkiem i gdzie stoimy mamy różne ograniczenia
        //(między innymi nie możemy sprawdzać poza wymiarami tablicy)
        private void isGrabPossible()
        {
            hasToGrab = false;
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    if (column < 2)
                    {
                        if (row >= 2)
                        {
                            if (((board[row, column].state == "lightPawn") || (board[row, column].state == "lightKing")) && ((board[row - 1, column + 1].state == "darkPawn") || (board[row - 1, column + 1].state == "darkKing")) && (board[row - 2, column + 2].state == "empty"))
                            {
                                hasToGrab = true;
                            }
                        }
                        if (row <= 5)
                        {
                            if ((board[row, column].state == "lightKing") && ((board[row + 1, column + 1].state == "darkPawn") || (board[row + 1, column + 1].state == "darkKing")) && (board[row + 2, column + 2].state == "empty"))
                            {
                                hasToGrab = true;
                            }
                        }
                    }
                    if ((column >= 2) && (column < 6))
                    {
                        if (row >= 2)
                        {
                            if (((board[row, column].state == "lightPawn") || (board[row, column].state == "lightKing")) && ((((board[row - 1, column + 1].state == "darkPawn") || (board[row - 1, column + 1].state == "darkKing")) && (board[row - 2, column + 2].state == "empty")) || (((board[row - 1, column - 1].state == "darkPawn") || (board[row - 1, column - 1].state == "darkKing")) && (board[row - 2, column - 2].state == "empty"))))
                            {
                                hasToGrab = true;
                            }
                        }
                        if (row <= 5)
                        {
                            if ((board[row, column].state == "lightKing") && ((((board[row + 1, column + 1].state == "darkPawn") || (board[row + 1, column + 1].state == "darkKing")) && (board[row + 2, column + 2].state == "empty")) || (((board[row + 1, column - 1].state == "darkPawn") || (board[row + 1, column - 1].state == "darkKing")) && (board[row + 2, column - 2].state == "empty"))))
                            {
                                hasToGrab = true;
                            }
                        }
                    }
                    if (column >= 6)
                    {
                        if (row >= 2)
                        {
                            if (((board[row, column].state == "lightPawn") || (board[row, column].state == "lightKing")) && ((board[row - 1, column - 1].state == "darkPawn") || (board[row - 1, column - 1].state == "darkKing")) && (board[row - 2, column - 2].state == "empty"))
                            {
                                hasToGrab = true;
                            }
                        }
                        if (row <= 5)
                        {
                            if ((board[row, column].state == "lightKing") && ((board[row + 1, column - 1].state == "darkPawn") || (board[row + 1, column - 1].state == "darkKing")) && (board[row + 2, column - 2].state == "empty"))
                            {
                                hasToGrab = true;
                            }
                        }
                    }
                }
            }
            if (hasToGrab == true)
            {
                label3.Text = "Masz bicie, musisz je wykonać";
            }
        }
        //wykonanie bicia
        private void doGrab()
        {
            setEmpty((wherePawnMoveRow + whichPawnMoveRow)/2, (wherePawnMoveColumn + whichPawnMoveColumn)/2);
            label3.Text = "";
        }
        //sprawdzenie, czy gra została zakończona
        public void checkIfGameIsOver()
        {
            if (numberOfDarkPawns == 0)
            {
                whoseTurnLabel.Text = "Wygrałeś";
            }
            if (numberOfLightPawns == 0)
            {
                whoseTurnLabel.Text = "Przegrałeś";
            }
        }
        //zamknięcie aplikacji wraz z wywołanymi przez nią procesami po zamknięciu okna z grą
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
