using Kitchen.Services;

namespace Kitchen.Util
{
    public class CooksRankComparer : IComparer<Cook>
    {
        public int Compare(Cook? x, Cook? y)
        {
            if (x is null || y is null) throw new ArgumentException("One of cooks is null");
            return x.Rank.CompareTo(y.Rank);
        }
    }
}
