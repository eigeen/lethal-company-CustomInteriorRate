using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Interior
{
    public class Interior
    {
        public static string? ParseName(int id)
        {
            if (Enum.IsDefined(typeof(InteriorEnum), id))
            {
                var interiorType = (InteriorEnum)id;
                return interiorType.ToString();
            }
            return null;
        }

        public static int? ParseId(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (Enum.TryParse(typeof(InteriorEnum), name, out var result))
            {
                return (int)result;
            }

            return null;
        }

        public static InteriorEnum? ParseEnumFromId(int id)
        {
            if (Enum.IsDefined(typeof(InteriorEnum), id))
            {
                return (InteriorEnum)id;
            }
            return null;
        }

        public enum InteriorEnum
        {
            Factory = 0,
            Manor = 1,
            Mineshaft = 4,
        }
    }
}
