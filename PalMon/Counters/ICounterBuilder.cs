using PalMon.Helpers;

namespace PalMon.Counters
{
    public interface ICounterBuilder
    {
        ICounterBuilder CreateCounter(Host host);
        ICounterBuilder WithCategoryName(string category);
        ICounterBuilder WithCounterName(string counter);
        ICounterBuilder WithInstanceName(string instance);
        ICounterBuilder WithUnit(string unit);
    }
}