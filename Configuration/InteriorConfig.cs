using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Interior;

namespace Plugin.Configuration
{
    public class InteriorConfig
    {
        public int Id { get; set; }
        public string? Name
        {
            get
            {
                return Interior.Interior.ParseName(Id);
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                int? parsed = Interior.Interior.ParseId(value);
                if (parsed != null)
                {
                    Id = parsed.Value;
                }
            }
        }
        public double Rate { get; set; }
    }
}
