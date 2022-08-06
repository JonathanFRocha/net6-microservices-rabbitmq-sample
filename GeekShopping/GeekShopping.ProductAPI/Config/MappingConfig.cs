using AutoMapper;
using GeekShopping.ProductAPI.Model;
using GeekShopping.ProductAPI.Model.Data.ValueObjects;

namespace GeekShopping.ProductAPI.Config
{
    public class MappingConfig
    {

        public static MapperConfiguration RegisterMaps()
        {

            var mappingConfig =  new MapperConfiguration(config =>
            {
                config.CreateMap<ProductVO, Product>();
                config.CreateMap<Product, ProductVO>();
            });
            return mappingConfig;
        }
    }
}
