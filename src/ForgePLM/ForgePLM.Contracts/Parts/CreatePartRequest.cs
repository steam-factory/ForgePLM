using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgePLM.Contracts.Parts;

public sealed record CreatePartRequest(
    string ProjectCode,
    int EcoId,
    string CategoryCode,
    string Description,
    string DocumentType
);
