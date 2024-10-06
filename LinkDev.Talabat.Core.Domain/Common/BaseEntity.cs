﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Common
{
    public abstract class BaseEntity<Tkey> where Tkey : IEquatable<Tkey>
    {
        #region CommonFields 

        public required Tkey Id { get; set; }

        public required string CreatedBy { get; set; }

        public required string LastModifiedBy { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime LastModifiedOn { get; set; } = DateTime.UtcNow;


        #endregion

    }
}