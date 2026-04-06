using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgePLM.Contracts.Parts;

public sealed record CreatePartRequest(
    string ProjectCode,
    string EcoNumber,
    string CategoryCode,
    string Description
);
