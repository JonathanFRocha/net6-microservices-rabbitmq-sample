using GeekShopping.Web.Models;

namespace GeekShopping.Web.Services.IServices
{
    public interface IProductService
    {
        Task<IEnumerable<ProductViewModel>> FindAllProducts();
        Task<ProductViewModel> FindProductById(string token, long id);
        Task<ProductViewModel> CreateProduct(string token, ProductViewModel product);
        Task<ProductViewModel> UpdateProduct(string token, ProductViewModel product);
        Task<bool> DeleteProductById(string token, long id);
    }
}
