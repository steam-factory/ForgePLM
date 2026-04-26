using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Projects;


public sealed record UpdateProjectRequest(
    string ProjectName,
    bool IsActive
);
