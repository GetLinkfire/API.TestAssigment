using System.Collections.Generic;

namespace Service.Models.StorageModel.Music
{
    public sealed class StorageModel : Base.StorageModel
	{
		public TrackingStorageModel TrackingInfo { get; set; }

        public Dictionary<string, List<DestinationStorageModel>> Destinations { get; set; }
    }
}
