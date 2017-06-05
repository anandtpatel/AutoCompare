using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoCompare.Console
{
    public class Consumer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address Address { get; set; }
        public List<string> ParentList { get; set; }
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class Address
    {
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public List<string> SomeList { get; set; }
    }
}
