using System;
using System.Collections.Generic;

namespace Platformex
{
    public abstract class Entity<TIdentity> : ValueObject, IEntity<TIdentity>
    {
        protected Entity(TIdentity id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public TIdentity Id { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}