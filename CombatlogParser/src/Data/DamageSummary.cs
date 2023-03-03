using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatlogParser.Data
{
    public class DamageSummary
    {
        public string SourceName { get; set; }
        public long TotalDamage { get; set; }
        public float DPS { get; set; }
    }
}
