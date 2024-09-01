using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saltworks.EventHandling
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T EventData { get; private set; }

        public GenericEventArgs(T EventData)
        {
            this.EventData = EventData;
        }
    }

    public class GenericEventArgs<T,U> : EventArgs
    {
        public T First { get; private set; }
        public U Second { get; private set; }

        public GenericEventArgs(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }
    }
}
