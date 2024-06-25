using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Shared.ResponseModels
{
    public class LuckyWheelResponse
    {
        public int LuckyNumber { get; set; }
        public DateTime LastTimeWheelSpin { get; set; }
    }
}
