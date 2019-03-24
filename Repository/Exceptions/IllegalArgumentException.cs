using System;

namespace Repository.Exceptions
{
    public class IllegalArgumentException : Exception
    {
        public IllegalArgumentException(Type entityType, object id) : base($"Id {id} is illegal for {entityType.Name}.")
        {
        }
    }
}
