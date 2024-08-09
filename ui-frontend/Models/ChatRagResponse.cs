using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ui_frontend.Models
{
    public class ChatRagResponse
    {
        public string? Content { get; set; }
        public MetaData? MetaData { get; set; }
        public List<Citation>? Citations { get; set; }
    }
    public class MetaData
    {
        public int CompletionTokens { get; set; }
        public int PromptTokens { get; set; }
        public int TotalTokens { get; set; }
    }
    public class Citation
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? FilePath { get; set; }
    }
}
