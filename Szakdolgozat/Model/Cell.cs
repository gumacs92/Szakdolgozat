using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdolgozat.Model
{
    public class Coordinates
    {
        public Coordinates(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public int X { get; set; }
        
        public int Y { get; set; }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            if (X == ((Coordinates)obj).X && Y == ((Coordinates)obj).Y)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class Cell
    {   
        public Cell()
        {
            Value = 0;
            Merged = false;
            From = new List<Coordinates>();
        }
        
        public int Value { get; set; }
        
        public bool Merged { get; set; }

        public List<Coordinates> from;
        public List<Coordinates> From
        {   get
            {
                return from;
            }

            set
            {
                from = value;
            }
        }

        public bool Created { get; set; }

    }
}
