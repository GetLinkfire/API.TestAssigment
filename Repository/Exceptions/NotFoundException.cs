using System;

namespace Repository.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(Type entityType, object id) : base($"{entityType.Name} with Id {id} not found.")
        {
        }
    }
}
