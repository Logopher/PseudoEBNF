using System;

namespace PseudoEBNF.Common
{
    public abstract class Compatible
    {
        public Guid CompatibilityGuid { get; }

        public Compatible(Guid guid)
        {
            CompatibilityGuid = guid;
        }

        public Compatible(Compatible c)
            : this(c.CompatibilityGuid)
        { }

        public bool IsCompatibleWith(Compatible other)
        {
            if (ReferenceEquals(other, null))
            { throw new NullReferenceException(); }

            return CompatibilityGuid == other.CompatibilityGuid;
        }
    }
}