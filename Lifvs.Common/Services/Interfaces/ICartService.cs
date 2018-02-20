using Lifvs.Common.ApiModels;
using System.Collections.Generic;

namespace Lifvs.Common.Services.Interfaces
{
    public interface ICartService
    {
        long GenerateCart(string userId, string storeId);
        List<AddCartItemResponeseModel> AddCartItem(string userId, string cartId, string storeId, OfflineCartItemModel inventoryId);
        void DeleteCartItem(string cartItemId, string storeId, string inventoryId);
        string AddOfflineCartItem(string userId, string cartId, string storeId, OfflineCartItemModel lstInventoryIds);
        CartItemModel AddCartItemForCustomerShop(string userId, string cartId, string storeId, string inventoryId);
        List<CartItemModel> GetAllCartItemList(string cartId);
        bool RemoveAllCartItems(long userId, long storeId, long cartId);
        bool CancelShopping(long userId, long storeId, long cartId);
    }
}
