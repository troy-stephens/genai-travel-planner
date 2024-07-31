using System;
using System.Collections.Generic;

namespace api_sanitize_validate.Models;
public class HarmfulCheckResult
{
    public string? Vendor { get; set; }
    public string? Date { get; set; }
    public bool HarmfulContentFound { get; set; }
    public List<OffendingElement>? OffendingElements { get; set; }
}

public class OffendingElement
{
    public string? Id { get; set; }
    public string? HarmCategory { get; set; }
    public string? Element { get; set; }
    public string? Content { get; set; }
}