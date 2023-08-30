using Rain.Model;

namespace Rain.Quartz
{
    public interface IAlphaVantageSynchronizer
    {
        public  Task<BatchPageResponse> Synchronize(int startIndex);
    }
}