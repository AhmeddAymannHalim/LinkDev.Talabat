﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Common
{

    public interface IBaseAuditableEntity
    {
        public string? CreatedBy { get; set; } 

        public string? LastModifiedBy { get; set; } 

        public DateTime? CreatedOn { get; set; } 

        public DateTime? LastModifiedOn { get; set; }

    }

    public abstract class BaseAuditableEntity<Tkey> : BaseEntity<Tkey> , IBaseAuditableEntity
        where Tkey : IEquatable<Tkey>
    {
        #region CommonFields 


        public string? CreatedBy { get; set; } = null!;

        public string? LastModifiedBy { get; set; } = null!;

        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? LastModifiedOn { get; set; } = DateTime.UtcNow;


        #endregion

    }
}
