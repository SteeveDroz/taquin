using Cpln.SteeveDroz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace taquin
{
    public partial class Taquin : Form
    {
        private Timer timer = new Timer();
        private Piece movingPiece;
        private double movingPosition = 0;
        private int backdoor = 0;

        public Point PiecesSize { get; set; }

        public List<Piece> Pieces { get; set; }

        public Piece EmptyPiece { get; private set; }

        public int Moves { get; private set; }

        public bool Start { get; private set; }

        public Taquin() : this(2, 2) { }

        public Taquin(int width, int height)
        {
            InitializeComponent();
            ClientSize = new Size(420, 420);
            DoubleBuffered = true;
            timer.Tick += new EventHandler(timer_Tick);

            Pieces = new List<Piece>();

            this.PiecesSize = new Point(width, height);
            GeneratePieces();
        }

        private void GeneratePieces()
        {
            Pieces = new List<Piece>();
            int id = 1;
            for (int y = 0; y < PiecesSize.Y; y++)
            {
                for (int x = 0; x < PiecesSize.X; x++)
                {
                    if (id == PiecesSize.X * PiecesSize.Y)
                    {
                        EmptyPiece = new Piece(x, y, this);
                    }
                    else
                    {
                        Pieces.Add(new Piece(x, y, id, this));
                    }
                    id++;
                }
            }
            Shuffle();
        }

        public void Shuffle()
        {
            do
            {
                foreach (Piece piece in Pieces)
                {
                    if (piece.Location == new Point(PiecesSize.X - 1, PiecesSize.Y - 1))
                    {
                        Swap(piece, EmptyPiece);
                        break;
                    }
                }
                ShuffleList<Piece> shuffledList = new ShuffleList<Piece>();
                foreach (Piece piece in Pieces)
                {
                    shuffledList.Add(piece);
                }
                shuffledList.Shuffle();

                for (int i = 0; i < shuffledList.Count; i++)
                {
                    Pieces[i].Location = shuffledList[i].Destination;
                }

                if (!IsSolvable())
                {
                    Swap(shuffledList[0], shuffledList[1]);
                }
            } while (IsSolved());

            Moves = 0;
            Start = true;
        }

        private void Taquin_Paint(object sender, PaintEventArgs e)
        {
            int pieceWidth = ClientRectangle.Width / PiecesSize.X;
            int pieceHeight = ClientRectangle.Height / PiecesSize.Y;

            Graphics g = e.Graphics;

            foreach (Piece piece in Pieces)
            {
                if (piece != movingPiece)
                {
                    if (piece.Location == piece.Destination)
                    {
                        g.FillRectangle(Brushes.Blue, PieceCoordinates(piece));
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Red, PieceCoordinates(piece));
                    }
                    g.DrawString("" + piece.Id, new Font("Arial", 16), Brushes.Black, piece.Location.X * pieceWidth, piece.Location.Y * pieceHeight);
                }
            }
            if (movingPiece != null)
            {
                int x = (int)((EmptyPiece.Location.X + movingPosition * (movingPiece.Location.X - EmptyPiece.Location.X)) * ClientRectangle.Width / PiecesSize.X);
                int y = (int)((EmptyPiece.Location.Y + movingPosition * (movingPiece.Location.Y - EmptyPiece.Location.Y)) * ClientRectangle.Height / PiecesSize.Y);
                g.FillRectangle(Brushes.Red, x, y, ClientRectangle.Width / PiecesSize.X - 1, ClientRectangle.Height / PiecesSize.Y - 1);
                g.DrawString("" + movingPiece.Id, new Font("Arial", 16), Brushes.Black, x, y);
            }
            g.DrawString("Coups : " + Moves, new Font("Arial", 16), Brushes.Black, 0, ClientRectangle.Height - 25);
        }

        private Rectangle PieceCoordinates(Piece piece)
        {
            return new Rectangle(piece.Location.X * ClientRectangle.Width / PiecesSize.X, piece.Location.Y * ClientRectangle.Height / PiecesSize.Y, ClientRectangle.Width / PiecesSize.X - 1, ClientRectangle.Height / PiecesSize.Y - 1);
        }
        
        private bool IsSolvable()
        {
            List<Piece> pieces = Pieces.Select(piece => piece.Clone()).ToList();
            int swaps = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                for (int j = i + 1; j < pieces.Count; j++)
                {
                    if (pieces[i].Destination == pieces[j].Location && pieces[i].Destination != pieces[j].Destination)
                    {
                        Swap(pieces[i], pieces[j]);
                        swaps++;
                        break;
                    }
                }
            }
            return swaps % 2 == 0;
        }

        private void Swap(Piece piece1, Piece piece2)
        {
            Point location = piece1.Location;
            piece1.Location = piece2.Location;
            piece2.Location = location;
        }

        private void Taquin_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                Piece piece = EmptyPiece;
                switch (e.KeyCode)
                {
                    case Keys.Down:
                        piece = FindPiece(EmptyPiece.Location.X, EmptyPiece.Location.Y - 1);
                        break;

                    case Keys.Up:
                        piece = FindPiece(EmptyPiece.Location.X, EmptyPiece.Location.Y + 1);
                        break;

                    case Keys.Right:
                        piece = FindPiece(EmptyPiece.Location.X - 1, EmptyPiece.Location.Y);
                        break;

                    case Keys.Left:
                        piece = FindPiece(EmptyPiece.Location.X + 1, EmptyPiece.Location.Y);
                        break;

                    case Keys.S:
                        if (Start)
                        {
                            ChangeSize(-1);
                        }
                        Shuffle();
                        Invalidate();
                        return;


                    default:
                        return;
                }

                Swap(piece, EmptyPiece);
                SlowMove(piece);
                Moves++;
                Start = false;
                backdoor = 0;
            }
            catch (PieceNotFoundException) {
                backdoor++;
                if (backdoor >= 5)
                {
                    ChangeSize(1);
                }
            }
        }

        private void ChangeSize(int delta)
        {
            Point size = PiecesSize;
            size.Offset(delta, delta);
            if (size.X > 1 && size.Y > 1)
            {
                PiecesSize = size;
                GeneratePieces();
                Invalidate();
            }
        }

        private Piece FindPiece(int x, int y)
        {
            Point location = new Point(x, y);
            foreach (Piece piece in Pieces)
            {
                if (piece.Location == location)
                {
                    return piece;
                }
            }

            throw new PieceNotFoundException();
        }

        private void SlowMove(Piece piece)
        {
            movingPiece = piece;
            movingPosition = 0;
            timer.Interval = 10;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            movingPosition += 0.1;
            Invalidate();
            if (movingPosition > 1)
            {
                timer.Stop();
                movingPiece = null;
                movingPosition = 0;
                if (IsSolved())
                {
                    MessageBox.Show("Terminé en " + Moves + " coups.", "Bravo");
                    ChangeSize(1);
                }
            }
        }

        public bool IsSolved()
        {
            foreach (Piece piece in Pieces)
            {
                if (piece.Location != piece.Destination)
                {
                    return false;
                }
            }
            return true;
        }

        private void Taquin_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
