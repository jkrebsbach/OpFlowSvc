using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class SecureImage
    {
        public int ImageID { get; set; }
        public string DocumentBytes { get; set; }
    }

    public class CompositeSummary
    {
        public object Summary { get; set; }
        public List<SecureImage> Images { get; set; }
    }
}
