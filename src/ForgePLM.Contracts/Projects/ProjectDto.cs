using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Projects;

public sealed record ProjectDto(
    int ProjectId,
    int CustomerId,
    int ProjectSeq,
    string ProjectCode,
    string ProjectName,
    string? ProjectDescription,
    bool IsActive
);
