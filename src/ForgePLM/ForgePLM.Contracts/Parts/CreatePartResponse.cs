using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgePLM.Contracts.Parts;

public record CreatePartResponse(
    int PartId,
    int PartNumberInt,
    string PartNumber,
    int ProjectId,
    string CategoryCode,
    string CategoryName,
    string? DescriptionCurrent,
    int? CurrentRevisionId,
    DateTime CreatedAt,
    DateTime? RetiredAt)
{
    public string DisplayPartNumber => $"{CategoryCode}-{PartNumberInt:D7}";
}