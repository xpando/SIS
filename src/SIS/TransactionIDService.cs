using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SIS
{
    public class TransactionIDService
    {
        private readonly object _syncRoot = new Object();
        private Block _currentBlock = null;
        private Block _nextBlock = null;
        public const ulong BlockSize = 8000; // should use multiple of 8 for good block alignment
        public const ulong TotalBlocks = TransactionID.MaxValue / BlockSize;

        public Block Current
        {
            get
            {
                return GetCurrentBlock();
            }
        }

        Task _blockAllocationTask;

        public TransactionIDService()
        {
            // Allocate the initial current block and
            // pre-allocate the next block on a background thread
            _currentBlock = AllocateBlock();
            _blockAllocationTask = Task.Factory.StartNew(AllocateNextBlock);
        }

        public TransactionID GetTransactionID()
        {
            TransactionID id;
            var block = GetCurrentBlock();
            if (!block.Next(out id))
                throw new InvalidOperationException("Block failure");
            return id;
        }

        Block GetCurrentBlock()
        {
            if (_currentBlock.IsConsumed)
            {
                // if next block is being allocated then wait
                // This has the potential to block clients getting ids
                // TODO: use a ConcurrentQueue to queue up multiple blocks
                // track some call time stats and block allocation time stats
                // so that we can estimate how many blocks we should be allocating
                // and inserting into the block queue to keep up with the front end calls
                _blockAllocationTask.Wait();
                if (_currentBlock.IsConsumed)
                {
                    _currentBlock = _nextBlock;
                    _blockAllocationTask = Task.Factory.StartNew(AllocateNextBlock);
                }
            }

            return _currentBlock;
        }

        Block AllocateBlock()
        {
            var block = GetRandomBlock();
            while (!LockBlock(block))
                block = GetRandomBlock();
            return block;
        }

        void AllocateNextBlock()
        {
            _nextBlock = AllocateBlock();
        }

        Block GetRandomBlock()
        {
            var bytes = new byte[8];
            _random.GetBytes(bytes);
            var blockIndex = BitConverter.ToUInt64(bytes, 0) % TotalBlocks;
            var start = blockIndex * BlockSize;
            return new Block(start, start + BlockSize - 1);
        }

        bool LockBlock(Block block)
        {
            // TODO: obtain lock on block using a persistent store like MongoDB or DynamoDB
            // Simulate expensive block allocation step
            Thread.Sleep(500);
            return true;
        }

        static RandomNumberGenerator _random = RandomNumberGenerator.Create();
    }
}