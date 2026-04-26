using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgePLM.Contracts.PartCategories;

public record PartCategoryDto(
    string CategoryCode,
    string CategoryName,
    string? Guideline,
    bool IsActive
);