using Platformex;

namespace Siam.MemoContext
{

    public class DocumentNumber : SingleValueObject<string>
    {
        public DocumentNumber(string value) : base(value) { }
    }

    public class Address : ValueObject
    {
        public Address(string index, string country, string city, string street, string building)
        {
            Index = index;
            Country = country;
            City = city;
            Street = street;
            Building = building;
        }

        public string Index { get; }
        public string Country { get; }
        public string City { get; }
        public string Street { get; }
        public string Building { get; }
    }
    public class MemoDocument : Entity<string>
    {
        public DocumentNumber Number { get; private set; }

        public Address CustomerAddress { get; private set; }

        public MemoDocument(string id, DocumentNumber number, Address customerAddress) : base(id)
        {
            Number = number;
            CustomerAddress = customerAddress;
        }
    }
    
}