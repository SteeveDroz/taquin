using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpln.SteeveDroz
{
    class ShuffleList<T> : List<T>
    {
        public void Shuffle()
        {
            Random random = new Random();
            for (int count = Count; count > 0; count--)
            {
                int i = random.Next(count);
                Add(this[i]);
                RemoveAt(i);
            }
        }
    }
}
