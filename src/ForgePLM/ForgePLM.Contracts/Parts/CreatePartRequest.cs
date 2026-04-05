using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgePLM.Contracts.Parts;

public record CreatePartRequest(
    string ProjectCode,
    string CategoryCode,
    string? Description
);
