using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lifvs.Common.ApiModels;

namespace Lifvs.Common.Services
{
   public class CartService : ICartService
    {
        private readonly ILog _log;
        private readonly ICartRepository _cartRepository;
        private readonly IExceptionManager _exceptionManager;

        public CartService(ILog log, ICartRepository cartRepository, IExceptionManager exceptionManager)
        {
            _log = log;
            _cartRepository = cartRepository;
            _exceptionManager = exceptionManager;
        }

        public List<AddCartItemResponeseModel> AddCartItem(string userId, string cartId, string storeId, OfflineCartItemModel inventoryId)
        {
            return _cartRepository.AddCartItem(userId, cartId, storeId, inventoryId);
        }

        public CartItemModel AddCartItemForCustomerShop(string userId, string cartId, string storeId, string inventoryCode)
        {
            return _cartRepository.AddCartItemForCustomerShop(userId, cartId, storeId, inventoryCode);
        }

        public string AddOfflineCartItem(string userId, string cartId, string storeId, OfflineCartItemModel model)
        {
            return _cartRepository.AddOfflineCartItem(userId, cartId, storeId, model);
        }

        public void DeleteCartItem(string cartItemId, string storeId, string inventoryId)
        {
            _cartRepository.DeleteCartItem(cartItemId, storeId, inventoryId);
        }

        public long GenerateCart(string userId, string storeId)
        {
            return _cartRepository.GenerateCart(userId, storeId);
        }

        public List<CartItemModel> GetAllCartItemList(string cartId)
        {
            return _cartRepository.GetAllCartItemList(cartId);
        }
        public bool RemoveAllCartItems(long userId, long storeId, long cartId)
        {
            var isCartExistForUserAndStore = _cartRepository.IsCartExistForUserAndStore(userId, storeId);
            if (!isCartExistForUserAndStore)
                throw _exceptionManager.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig korg som inte är kopplad till användare och butik.");

            var totalQuantitiesByCart = _cartRepository.GetTotalQuantityByCartId(cartId);
            foreach(var item in totalQuantitiesByCart)
            {
                _cartRepository.UpdateStockForRemovingCart(item.TotalQuantity, storeId.ToString(), item.InventoryId.ToString());
            }
            return _cartRepository.RemoveCartItemsByCartId(cartId);
        }
        public bool CancelShopping(long userId, long storeId, long cartId)
        {
            var isCartExistForUserAndStore = _cartRepository.IsCartExistForUserAndStore(userId, storeId);
            if (!isCartExistForUserAndStore)
                throw _exceptionManager.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig korg som inte är kopplad till användare och butik.");

            var cartObj = _cartRepository.GetCartDetailById(cartId);
            if (cartObj == null)
                throw _exceptionManager.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Köpkorgen existerar inte.");

            var cartItem = _cartRepository.GetCartItemByCartId(cartId);
            if (cartItem == null)
            {
                if (DateTime.Now.Subtract(cartObj.CartGeneratedDate).TotalMinutes <= CommonConstants.CartExpireTime)
                {
                    if (cartObj.PaymentDone)
                        throw _exceptionManager.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Betalning har redan gjorts för denna korg.");
                    return _cartRepository.RemoveCart(cartId);
                }
            }
            else
            {
                if (DateTime.Now.Subtract(cartItem.CreatedDate).TotalMinutes <= CommonConstants.CartExpireTime)
                {
                    if(cartObj.PaymentDone)
                        throw _exceptionManager.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Betalning har redan gjorts för denna korg.");

                    var totalQuantitiesByCart = _cartRepository.GetTotalQuantityByCartId(cartId);
                    foreach (var item in totalQuantitiesByCart)
                    {
                        _cartRepository.UpdateStockForRemovingCart(item.TotalQuantity, storeId.ToString(), item.InventoryId.ToString());
                    }

                    return _cartRepository.RemoveCartItemsByCartId(cartId);
                }
                else
                {
                    if (!cartObj.PaymentDone)
                    {
                        _cartRepository.MakePayment(userId.ToString(), storeId.ToString(), cartId.ToString());
                    }
                }
            }
            return true;
        }
    }
}
