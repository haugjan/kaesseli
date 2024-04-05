using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaesseli.Domain.Automation
{
    public class AutomationEntry
    {
        public required Guid Id { get; init; }
        public required string AutomationText { get; init; }
        public required IEnumerable<AutomationEntryPart> Parts { get; init; }
    }
}
