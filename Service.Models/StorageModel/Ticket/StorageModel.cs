using System.Collections.Generic;

namespace Service.Models.StorageModel.Ticket
{
    public sealed class StorageModel : Base.StorageModel
    {
        public Dictionary<string, List<DestinationStorageModel>> Destinations { get; set; }
    }
}
