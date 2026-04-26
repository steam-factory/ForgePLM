using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Requests
{
    public sealed record UpdateRevisionDescriptionRequest(
        string Description
    );
}