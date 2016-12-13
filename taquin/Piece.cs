using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace taquin
{
    public class Piece
    {
        private Taquin parent;

        private Point destination;
        private Point location;

        public Point Destination
        {
            get
            {
                return destination;
            }
            private set
            {
                destination = value;
            }
        }

        public Point Location
        {
            get
            {
                return location;
            }
            set
            {
                if (value.X < parent.PiecesSize.X && value.Y < parent.PiecesSize.Y)
                {
                    location = value;
                }
            }
        }

        public int Id { get; set; }

        public Piece(int x, int y, Taquin parent) : this(x, y, 0, parent) { }

        public Piece(int x, int y, int id, Taquin parent)
        {
            this.parent = parent;

            this.Location = new Point(x, y);
            this.Destination = new Point(x, y);
            this.Id = id;
        }

        private Piece() { }

        public Piece Clone()
        {
            Piece clone = new Piece();

            clone.parent = parent;
            clone.destination = destination;
            clone.location = location;

            clone.Id = Id;

            return clone;
        }
    }
}
