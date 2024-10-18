﻿namespace LinkDev.Talabat.Core.Domain.Entities._Identity
{
    public class Address
    {

        public int Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Street { get; set; }

        public required string City { get; set; }

        public required string Country { get; set; }

        public string UserId { get; set; }

        public virtual required ApplicationUser User { get; set; }

    }
}