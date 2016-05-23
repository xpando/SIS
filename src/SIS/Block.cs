using System;
using System.Threading;

namespace SIS
{
    public class Block
    {
        private long _current;

        public ulong Start
        {
            get;
            private set;
        }

        public ulong End
        {
            get;
            private set;
        }

        public bool IsConsumed
        {
            get
            {
                return (ulong)_current >= End;
            }
        }

        public Block(ulong start, ulong end)
        {
            _current = (long)start;
            Start = start;
            End = end;
        }

        public bool Next(out TransactionID transactionID)
        {
            Interlocked.Increment(ref _current);
            transactionID = (ulong)_current;
            return ((ulong)_current <= End);
        }
    }
}