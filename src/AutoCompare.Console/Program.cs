using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoCompare.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var consumer = LoadConsumer();
            var consumer2 = LoadConsumer();
            consumer2.Age = 3;
            consumer2.LastName = "Shah";
            consumer2.Address.City = "Belmont";

            var differences = AutoCompare.Comparer.Compare<Consumer>(consumer, consumer2);
        }


        private  static Consumer LoadConsumer()
        {
            return new Consumer
            {
                FirstName = "Anand",
                LastName = "Patel",
                Age = 2,
                DateOfBirth = DateTime.Now,
                Address = new Address
                {
                    City = "Chalotte",
                    AddressLine = "10017 Paxton Run Road",
                    Zip = "28277"
                }
            };
        }
    }
}
