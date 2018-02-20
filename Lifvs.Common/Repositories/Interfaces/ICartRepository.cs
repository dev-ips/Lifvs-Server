using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface ICartRepository
    {
        long GenerateCart(string userId, string storeId);
        List<AddCartItemResponeseModel> AddCartItem(string userId, string cartId, string storeId, OfflineCartItemModel inventoryId);
        void DeleteCartItem(string cartItemId, string storeId, string inventoryId);
        string AddOfflineCartItem(string userId, string cartId, string storeId, OfflineCartItemModel lstInventoryIds);
        CartItemModel AddCartItemForCustomerShop(string userId, string cartId, string storeId, string inventoryId);
        List<CartItemModel> GetAllCartItemList(string cartId);
        bool IsCartExistForUserAndStore(long userId, long storeId);
        bool RemoveCartItemsByCartId(long cartId);
        Cart GetCartDetailById(long cartId);
        CartItem GetCartItemByCartId(long cartId);
        bool RemoveCart(long cartId);
        void MakePayment(string userId, string storeId, string cartId);
        List<TotalQuantityByCart> GetTotalQuantityByCartId(long cartId);
        void UpdateStockForRemovingCart(long units, string storeId, string inventoryId);
    }
}
