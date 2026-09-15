using System;
using System.Collections.Generic;
using System.Text;

namespace Reclevia.Core.Tenancy
{
    public class Tenant
    {
        public const int MaxNameLength = 200;

        private Tenant()
        { }

        public Tenant(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tenant name is required", nameof(name));

            var normalizedName = name.Trim();

            if ((normalizedName.Length > MaxNameLength))
                throw new ArgumentException("Tenant name cannot exceed 200 characters", nameof(name));

            Id = Guid.NewGuid();
            Name = normalizedName;
            CreatedAtUtc = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; private set; }
    }
}
