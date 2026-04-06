using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Projects;

public sealed record CreateProjectRequest(
    int CustomerId,
    string ProjectName,
    bool IsActive
);