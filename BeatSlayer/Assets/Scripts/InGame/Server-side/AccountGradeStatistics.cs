using Ranking;

namespace BeatSlayerServer.Models.Database
{
    public class AccountGradeStatistics
    {
        public int SS { get; set; }
        public int S { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }

        public void Apply(Grade grade)
        {
            switch (grade)
            {
                case Grade.SS: SS++; break;
                case Grade.S: S++; break;
                case Grade.A: A++; break;
                case Grade.B: B++; break;
                case Grade.C: C++; break;
                case Grade.D: D++; break;
            }
        }
    }
}
