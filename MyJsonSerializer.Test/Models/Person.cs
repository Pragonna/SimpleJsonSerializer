namespace MyJsonSerializer.Test.Models
{
    public class Person
    {
        public int Id { get; set; }
        public Guid GuidId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string[] Phones { get; set; }
        public List<string> Emails { get; set; }
        public Address Address { get; set; }
        public List<Address> Addresses { get; set; }
    }
}
