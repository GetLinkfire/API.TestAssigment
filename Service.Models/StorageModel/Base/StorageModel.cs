using System;
using Service.Models.Enums;

namespace Service.Models.StorageModel.Base
{
    public abstract class StorageModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public MediaType MediaType { get; set; }
    }
}
