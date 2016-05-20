using System;
using System.Security.Cryptography;
using System.Text;

namespace SIS
{
    public struct TransactionID
    {
        ulong _value;

        public TransactionID(ulong value)
        {
            if (value > MaxValue)
                throw new ArgumentException("Invalid TransactionID", nameof(value));

            _value = value;
        }

        public static implicit operator TransactionID(ulong value)
        {
            return new TransactionID(value);
        }

        public static implicit operator ulong(TransactionID value)
        {
            return value._value;
        }

        public override string ToString()
        {
            var value = _value;

            var digits = new byte[13];

            // build string buffer from right to left
            // 2-9 are encoded in 3 bits per digit. Mask and offset into ASCII starting at character '2'
            for (var i = 12; i >= 5; i--)
            {
                digits[i] = (byte)((value & 0x07) + 0x32);
                value >>= 3;
            }

            digits[4] = (byte)'-';

            // last 4 digits are base 23 encoded binary
            // get each digit and use as index into char map
            for (var i = 3; i >= 0; i--)
            {
                var idx = value % 23;
                digits[i] = _alpha[idx];
                value = value / 23;
            }

            return ASCIIEncoding.ASCII.GetString(digits);
        }

        public const ulong MinValue = 0UL;
        public const ulong MaxValue = 863871334088703UL; // (23^5 * 8^9)-1

        static byte[] _alpha = ASCIIEncoding.ASCII.GetBytes("ABCDEFGHJKMNPQRSTUVWXYZ");
    }

    public class Block
    {
        public ulong Start { get; private set; }
        public ulong End { get; private set; }
        public TransactionID Current { get; private set; }

        public Block(ulong start, ulong end)
        {
            Current = Start = start;
            End = end;
        }

        public bool Next()
        {
            if (Current < End - 1)
            {
                Current++;
                return true;
            }

            return false;
        }
    }

    public class BlockManager
    {
        public const ulong BlockSize = 8; // should use multiple of 8 for good block alignment
        public const ulong TotalBlocks = TransactionID.MaxValue / BlockSize; 

        public Block AllocateBlock()
        {
            var bytes = new byte[8];
            _random.GetBytes(bytes);
            var blockIndex = BitConverter.ToUInt64(bytes, 0) % TotalBlocks;
            var start = blockIndex * BlockSize;
            return new Block(start, start + BlockSize);
        }

        static RandomNumberGenerator _random = RandomNumberGenerator.Create();
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(((TransactionID)TransactionID.MinValue).ToString());
            Console.WriteLine();

            var blockManager = new BlockManager();

            var block = blockManager.AllocateBlock();
            do
            {
                Console.WriteLine(block.Current.ToString());
            } while (block.Next());

            Console.WriteLine();
            Console.WriteLine(((TransactionID)TransactionID.MaxValue).ToString());
        }
    }
}
