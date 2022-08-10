using Orders.Common.Dtos;

namespace Orders.Domain
{
    public class Address
    {
        public string Address1 { get; private set; }
        public string Address2 { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }

        public Address(string address1, string address2, string city, string state, string zipCode)
        {
            Address1 = address1;
            Address2 = address2;
            City = city;
            State = state;
            ZipCode = zipCode;
        }

        public AddressDto ToDto()
        {
            return new AddressDto
            {
                Address1 = Address1,
                Address2 = Address2,
                City = City,
                State = State,
                ZipCode = ZipCode
            };
        }

        public Address(AddressDto dto)
        {
            Address1 = dto.Address1;
            Address2 = dto.Address2;
            City = dto.City;
            State = dto.State;
            ZipCode = dto.ZipCode;
        }
    }
}