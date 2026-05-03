using ForgePLM.Contracts.Artifacts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Administrator.Models
{
    public sealed class ArtifactBatchHistoryRowViewModel
    {
        public ArtifactBatchDto Batch { get; set; }

        public string BatchCode => Batch.BatchCode;
        public string BatchDescription => Batch.BatchDescription;
        public string BatchState => Batch.BatchState;
        public IReadOnlyList<ArtifactDto> Artifacts => Batch.Artifacts;
        public string ZipStatus => string.IsNullOrWhiteSpace(Batch.ZipFilePath) ? "" : "ZIP";

        public ArtifactBatchHistoryRowViewModel(ArtifactBatchDto batch)
        {
            Batch = batch;
        }
    }
}
