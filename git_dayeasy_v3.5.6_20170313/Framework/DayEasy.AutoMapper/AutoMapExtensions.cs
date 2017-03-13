using AutoMapper;

namespace DayEasy.AutoMapper
{
    /// <summary> AutoMapper扩展 </summary>
    public static class AutoMapExtensions
    {
        public static TDestination MapTo<TDestination>(this object source)
        {
            return (source == null ? default(TDestination) : Mapper.Map<TDestination>(source));
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return (source == null ? default(TDestination) : Mapper.Map(source, destination));
        }
    }
}
