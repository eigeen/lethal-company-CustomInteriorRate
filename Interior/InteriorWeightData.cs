using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Interior
{
    public class InteriorWeightData
    {
        public int Id { get; set; }
        public string? Name
        {
            get
            {
                return Interior.ParseName(Id);
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                int? parsed = Interior.ParseId(value);
                if (parsed != null)
                {
                    Id = parsed.Value;
                }
            }
        }
        public int Weight { get; set; }
    }
}
