using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Projects
{
    public sealed record CreateProjectRequest(
        int CustomerId,
        string ProjectCode,
        string ProjectName,
        string? ProjectDescription
    );
}