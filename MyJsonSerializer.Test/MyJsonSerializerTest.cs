using FluentAssertions;
using MyJsonSerializer.Test.Models;

namespace MyJsonSerializer.Test
{
    public class MyJsonSerializerTest
    {
        private readonly Person person;

        public MyJsonSerializerTest()
        {
            person = new()
            {
                Name = "Test Name",
                Id = 1,
                GuidId = Guid.NewGuid(),
                Date = DateTime.Parse("30.01.2025 14:28:56"),
                Phones = new string[] { "123456", "654321" },
                Emails = new List<string> { "a@gm", "b@gm" },
                Address = new Address
                {
                    City = "Test City",
                    State = "TC",
                    Street = "123 Main St"
                },
                Addresses = new List<Address>
                {
                    new Address
                    {
                        City = "Test City",
                    State = "TC",
                    Street = "123 Main St"
                    },
                    new Address
                    {
                       City = "Test City",
                    State = "TC",
                    Street = "123 Main St"
                    }
                }
            };
        }
        [Fact]
        public void Serializer_WhenSerializeObject_ShouldNotBeNull()
        {
            var json = MyJsonSerializer.Serialize(person);

            json.Should().NotBeNull();
        }

        [Fact]
        public void Deserializer_WhenDeserializeObject_ShouldNotBeNull()
        {
            var json = MyJsonSerializer.Serialize(person);
            var _person = MyJsonSerializer.Deserialize<Person>(json);
            _person.Should().NotBeNull();
        }

        [Fact]
        public void Serializer_WhenSerializeObject_ShouldBeEqual()
        {
            var json = MyJsonSerializer.Serialize(person);
            var _person = MyJsonSerializer.Deserialize<Person>(json);
            _person.Should().BeEquivalentTo(person);
        }
    }
}