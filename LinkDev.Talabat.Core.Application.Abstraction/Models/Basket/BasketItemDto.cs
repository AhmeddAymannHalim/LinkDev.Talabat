﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Abstraction.Models.Basket
{
    public class BasketItemDto
    {
        public int Id { get; set; }

        public required string ProductName { get; set; }

        public string? ProductUrl { get; set; }

        [Range(.1,int.MaxValue,ErrorMessage ="Price must be greater than Zero!")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue,ErrorMessage ="Quantity must be at least one item")]

        public int Quantity { get; set; }

        public string? Brand { get; set; }

        public string? Category { get; set; }
    }
}
