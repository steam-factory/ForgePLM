using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.System;

public sealed class VaultPathSettings
{
    public string RootPath { get; set; } = string.Empty;
    public string ProjectsFolder { get; set; } = "Projects";
    public string DevelopmentFolder { get; set; } = "development";
    public string ProductionFolder { get; set; } = "production";
}
