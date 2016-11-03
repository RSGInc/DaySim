using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaySim {
    public interface ITripSkimWriter {
        void WriteSkimToFile(double[][] skim, string filename, int[] mapping, int count, int transport, int startTime,
                             int endTime, int factor);
    }
}
