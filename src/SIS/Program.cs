using System;
using System.Linq;

namespace SIS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Show min and max values for this ID space
            Console.WriteLine(((TransactionID)TransactionID.MinValue).ToString());
            Console.WriteLine(((TransactionID)TransactionID.MaxValue).ToString());
            Console.WriteLine();


            var service = new TransactionIDService();
            foreach (var i in Enumerable.Range(1, 20))
            {
                var id = service.GetTransactionID();
                Console.WriteLine(id.ToString());
            }
        }
    }
}