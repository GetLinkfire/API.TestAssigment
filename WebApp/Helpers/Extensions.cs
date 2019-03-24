using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace WebApp.Helpers
{
    internal static class Extensions
    {
        public static List<TDto> ToDtoList<TModel, TDto>(this Dictionary<string, List<TModel>> dictionary)
            where TModel : Service.Models.Link.Base.DestinationModel
            where TDto : Models.Base.DestinationDto
        {
            return dictionary?.SelectMany(x =>
                x.Value.Select(d =>
                {
                    var dest = Mapper.Map<TModel, TDto>(d);
                    dest.IsoCode = x.Key;
                    return dest;
                }))
                .ToList();
        }

        public static Dictionary<string, List<TModel>> ToModelDictionary<TModel, TDto>(this List<TDto> list)
            where TModel : Service.Models.Link.Base.DestinationModel
            where TDto : Models.Base.DestinationDto
        {
            return list?.GroupBy(x => x.IsoCode.ToUpper())
                .ToDictionary(x => x.Key, x => x.Select(Mapper.Map<TModel>)
                .ToList());
        }
    }
}