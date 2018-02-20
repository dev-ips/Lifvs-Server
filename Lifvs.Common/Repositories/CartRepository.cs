using Dapper;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using log4net;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Lifvs.Common.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ILog _log;
        private readonly IDbConnection _db;
        private readonly IExceptionManager _exception;
        private readonly IReceiptRepository _receiptRepository;
        private readonly ICryptoGraphy _cryptoGraphy;
        private readonly IAccessTokenRepository _user;

        public CartRepository(ILog log, IDbConnection db, IExceptionManager exception, IReceiptRepository receiptRepository, ICryptoGraphy cryptoGraphy,IAccessTokenRepository user)
        {
            _log = log;
            _db = db;
            _exception = exception;
            _receiptRepository = receiptRepository;
            _cryptoGraphy = cryptoGraphy;
            _user = user;
        }

        public long GenerateCart(string userId, string storeId)
        {
            var sqlQry = GetPaymentDoneForCartQry(userId, storeId);
            var cartData = _db.Query<CartModel>(sqlQry).FirstOrDefault();

            if (cartData == null)
            {
                // generate a new cart
                return GenerateNewCart(userId, storeId);
            }
            else
            {
                // check previous session is expired

                sqlQry = string.Empty;
                sqlQry = GetPreviousSessionTimeQry(cartData.Id);
                var cartItem = _db.Query<CartItemModel>(sqlQry).FirstOrDefault();

                if (cartItem == null)
                {
                    // check based on cart crated date

                    if (DateTime.Now.Subtract(cartData.CartGeneratedDate).TotalMinutes <= CommonConstants.CartExpireTime)
                    {
                        return cartData.Id;
                    }
                    else
                    {
                        // mark previous card payment as done and then generate new cart. No need to do payment process since not item added into the cart
                        UpdateCartPayment(cartData.Id, userId);

                        // Generate new cart
                        return GenerateNewCart(userId, storeId);
                    }
                }
                else if (DateTime.Now.Subtract(cartItem.CreatedDate).TotalMinutes <= CommonConstants.CartExpireTime)
                {
                    // if previous session active then return same cart
                    return cartData.Id;
                }
                else
                {
                    // need to generate receipt as well as transcation (auto payment function)
                    //var receiptId = _receiptRepository.AddReceiptItems(userId, storeId, cartData.Id.ToString());
                    //if (receiptId > 0)
                        Payment(userId, storeId, cartData.Id.ToString());

                    // mark previous card payment as done and then generate new cart
                    UpdateCartPayment(cartData.Id, userId);

                    // Generate new cart
                    return GenerateNewCart(userId, storeId);
                }
            }
        }
        public void MakePayment(string userId, string storeId, string cartId)
        {
            Payment(userId, storeId, cartId);
        }
        public void Payment(string userId, string storeId, string cartId)
        {
            try
            {
                var customers = new StripeCustomerService();
                var charges = new StripeChargeService();

                var userCardDetails = _receiptRepository.GetUserCardDetails(userId);
                if (userCardDetails == null)
                {
                    throw new Exception("Kreditkortsuppgifter existerar inte.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(userCardDetails.CreditCardId))
                    {
                        StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripPublishKey"]);
                        var tokenService = new StripeTokenService();
                        
                        
                        // Get token if user card is not created
                        var cardnumber = _cryptoGraphy.DecryptString(userCardDetails.CardNumber);

                        var token = tokenService.Create(new StripeTokenCreateOptions { Card = new StripeCreditCardOptions { Cvc = userCardDetails.CVC.ToString(), Number = cardnumber.Replace(" ", ""), ExpirationMonth = userCardDetails.ExpiredMonth, ExpirationYear = userCardDetails.ExpiredYear } });
                       
                        // Create customer in stripe
                        StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripSecretKey"]);
                        var customerId = customers.Create(new StripeCustomerCreateOptions { SourceToken = token.Id });

                        _receiptRepository.UpdateUserCreditCardId(customerId.Id, userId); //update customer id in database for future reference

                        var cartAmount = _receiptRepository.GetCartAmount(cartId);
                        var tmpTotalAmount = (int)cartAmount;

                        var vat1 = (tmpTotalAmount * CommonConstants.Vat1InPercentage) / 100;
                        var vat2 = (tmpTotalAmount * CommonConstants.Vat2InPercentage) / 100;
                        var totalAmount = tmpTotalAmount + vat1 + vat2;

                        // Payment process
                        if (totalAmount > 0)
                        {
                            var paymentResponse = charges.Create(new StripeChargeCreateOptions { Amount = totalAmount, Description = "Lifvs purchase item payment", Currency = "SEK", CustomerId = customerId.Id });

                            if (paymentResponse.Status.Equals("succeeded", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrWhiteSpace(paymentResponse.FailureMessage))
                            {
                                var receiptId = _receiptRepository.AddReceiptItems(userId, storeId, cartId);
                                if (receiptId > 0)
                                    _receiptRepository.CreateTransaction(userId, paymentResponse.Status, paymentResponse.Id, paymentResponse.StripeResponse.ResponseJson, receiptId.ToString());

                                var receiptItmes = _receiptRepository.GetAllReceiptItems(receiptId.ToString());
                                var user = _user.GetUser(Convert.ToInt64(userId));
                                SendEmail(user.Email, paymentResponse.Id, DateTime.Now.Date, storeId, receiptItmes);
                            }
                            else
                            {
                                throw new Exception(paymentResponse.FailureMessage);
                            }
                        }
                    }
                    else
                    {
                        StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripSecretKey"]);

                        var cartAmount = _receiptRepository.GetCartAmount(cartId);
                        var tmpTotalAmount = (int)cartAmount;
                        var vat1 = (tmpTotalAmount * CommonConstants.Vat1InPercentage) / 100;
                        var vat2 = (tmpTotalAmount * CommonConstants.Vat2InPercentage) / 100;
                        var totalAmount = tmpTotalAmount + vat1 + vat2;

                        // Payment process
                        if (totalAmount > 0)
                        {
                            var paymentResponse = charges.Create(new StripeChargeCreateOptions { Amount = totalAmount, Description = "Lifvs purchase item payment", Currency = "SEK", CustomerId = userCardDetails.CreditCardId });

                            if (paymentResponse.Status.Equals("succeeded", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrWhiteSpace(paymentResponse.FailureMessage))
                            {
                                var receiptId = _receiptRepository.AddReceiptItems(userId, storeId, cartId);
                                if (receiptId > 0)
                                    _receiptRepository.CreateTransaction(userId, paymentResponse.Status, paymentResponse.Id, paymentResponse.StripeResponse.ResponseJson, receiptId.ToString());

                                var receiptItmes = _receiptRepository.GetAllReceiptItems(receiptId.ToString());
                                var user = _user.GetUser(Convert.ToInt64(userId));
                                SendEmail(user.Email, paymentResponse.Id, DateTime.Now.Date, storeId, receiptItmes);
                            }
                            else
                            {
                                throw new Exception(paymentResponse.FailureMessage);
                            }
                        }
                    }
                }

            }
            catch (StripeException ex)
            {
                switch (ex.StripeError.ErrorType)
                {
                    case "card_error":
                        throw new Exception(ex.StripeError.Message);
                    case "api_connection_error":
                        break;
                    case "api_error":
                        break;
                    case "authentication_error":
                        break;
                    case "invalid_request_error":
                        break;
                    case "rate_limit_error":
                        break;
                    case "validation_error":
                        break;
                    default:
                        // Unknown Error Type
                        break;
                }
            }
        }

        public void SendEmail(string toMail, string paymentId, DateTime receiptDate, string storeId, List<ReceiptItemModel> receiptItmes)
        {
            try
            {
                var salesEmailId = WebConfigurationManager.AppSettings["SalesEmailId"];
                var fromMail = WebConfigurationManager.AppSettings["UserName"];
                var emailPassword = WebConfigurationManager.AppSettings["Password"];

                MailMessage message = new MailMessage();
                message.To.Add(new MailAddress(toMail));
                message.From = new MailAddress(fromMail);
                //message.CC.Add(new MailAddress(salesEmailId));
                message.Subject = "Lifvs - Purchase Receipt";
                message.Body = GenerateMailBody(paymentId, receiptDate, storeId, receiptItmes);
                message.IsBodyHtml = true;
                //if (!string.IsNullOrEmpty(AttachmentPath1))
                //    message.Attachments.Add(new Attachment(AttachmentPath1));

                SmtpClient smtp = new SmtpClient();
                smtp.Host = WebConfigurationManager.AppSettings["SmtpClient"];
                smtp.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["SmtpPort"]);
                smtp.Credentials = new System.Net.NetworkCredential(fromMail, emailPassword);
                smtp.EnableSsl = true;
                message.IsBodyHtml = true;
                smtp.Send(message);
            }
            catch
            {

            }
        }

        public string GenerateMailBody(string paymentId, DateTime receiptDate, string storeId, List<ReceiptItemModel> receiptItems)
        {
            var htmlBodyContent = string.Empty;
            var amount = 0;
            htmlBodyContent = "<!DOCTYPE html>";
            htmlBodyContent += "<html>";
            htmlBodyContent += "<body style='margin-top: 20px;'>";
            htmlBodyContent += "<div style='padding-right: 15px;padding-left: 15px;margin-right: auto;margin-left: auto;background-color:white;'>";
            htmlBodyContent += "<div style='margin-right: -15px;margin-left: -15px; '>";
            htmlBodyContent += "<div style='min-height: 20px;padding: 19px;margin-bottom: 20px;background-color: white;border: 1px solid #e3e3e3;border-radius: 4px;-webkit-box-shadow: inset 0 1px 1px rgba(0, 0, 0, .05);box-shadow: inset 0 1px 1px rgba(0, 0, 0, .05);position: relative;min-height: 1px;padding-right: 15px;padding-left: 15px; float:left;width:50%; margin-left: 8.33333333%; margin-left: 25%;'>";
            htmlBodyContent += "<div style='margin-right: -15px;margin-left: -15px; '>";
            htmlBodyContent += "<div style='width: 50%;float:left;position: relative;min-height: 1px;padding-right: 15px;padding-left: 15px;'>";

            htmlBodyContent += "</div>";

            htmlBodyContent += "<div style='width: 95%;float:left;position: relative;min-height: 1px;padding-right: 15px;padding-left: 15px;text-align:right;'>";
            htmlBodyContent += "<p>";
            htmlBodyContent += string.Format("<em>Butik : {0} </em>", storeId);
            htmlBodyContent += "</p>";
            htmlBodyContent += "<p>";
            htmlBodyContent += string.Format("<em>Date : {0} </em>", receiptDate.ToShortDateString());
            htmlBodyContent += "</p>";
            htmlBodyContent += "<p>";
            htmlBodyContent += string.Format("<em>Payment id #: {0} </em>", paymentId);
            htmlBodyContent += "</p>";

            htmlBodyContent += "</div>";
            htmlBodyContent += "</div>";

            htmlBodyContent += "<div style='margin-right: -15px;margin-left: -15px; '>"; // row

            htmlBodyContent += "<span>";
            htmlBodyContent += "<div style='text-align:center;'>";
            htmlBodyContent += "</div>";
            htmlBodyContent += "</span>";

            htmlBodyContent += "<table width='50%' style='background-color: #e8e8e8;border-radius: 5px;margin: 0px auto;float: none;box-sizing: border-box;border-collapse: collapse;width:70%;'>";
            htmlBodyContent += "<tbody style='background-color: white;'>";
            foreach (var item in receiptItems)
            {
                htmlBodyContent += "<tr style='border-bottom:1px solid #f4f4f4;'>";
                htmlBodyContent += "<td>" + item.Inventory + "";
                htmlBodyContent += "</td>";

                htmlBodyContent += "<td>" + Convert.ToInt32(item.Amount) + "";
                htmlBodyContent += "</td>";
                htmlBodyContent += "</tr>";

                amount += Convert.ToInt32(item.Amount);
            }

            htmlBodyContent += "<tr style='border-bottom:1px solid black;'>";
            htmlBodyContent += "<td><b> Total </b></td>";
            htmlBodyContent += "<td> <b>" + amount + " SEK </b></td>";
            htmlBodyContent += "</tr>";
            htmlBodyContent += "</tbody>";
            htmlBodyContent += "</table>";

            htmlBodyContent += "<div>";
            htmlBodyContent += "Best, <br />";
            htmlBodyContent += "Lifvs Team";
            htmlBodyContent += "</div>";

            htmlBodyContent += "</div>";

            htmlBodyContent += "</div>";
            htmlBodyContent += "</div>";
            htmlBodyContent += "</div>";
            htmlBodyContent += "</body>";
            htmlBodyContent += "</html>";
            return htmlBodyContent;
        }

        private long GenerateNewCart(string userId, string storeId)
        {
            // Generate new cart
            var sqlQry = string.Empty;
            sqlQry = GetGenerateCartQry();
            var cartId = _db.ExecuteScalar<long>(sqlQry, new
            {
                @cartDate = DateTime.Now,
                @userId = userId,
                @storeId = storeId,
                @createdDate = DateTime.Now,
                @createdBy = userId
            });
            return cartId;
        }

        private void UpdateCartPayment(long cartId, string userId)
        {
            var sqlQry = string.Empty;
            sqlQry = GetPreviousSessionCartPaymentQry(cartId, userId);
            _db.Query(sqlQry);
        }

        public List<AddCartItemResponeseModel> AddCartItem(string userId, string cartId, string storeId, OfflineCartItemModel inventoryId)
        {
            List<AddCartItemResponeseModel> list = new List<AddCartItemResponeseModel>();
            foreach (var item in inventoryId.InventoryIds)
            {
                // new cart item
                var sqlQry = string.Empty;
                sqlQry = GetInventoryPriceQry(item.ToString(), storeId); // get item price
                var itemPrice = _db.ExecuteScalar<decimal>(sqlQry);
                if (itemPrice > 0)
                {
                    sqlQry = string.Empty;
                    sqlQry = GetAddCartItemQry();
                    var cartItemId = _db.ExecuteScalar<long>(sqlQry, new
                    {
                        @cartId = cartId,
                        @inventoryId = item,
                        @quantity = 1,
                        @amount = itemPrice,
                        @createdDate = DateTime.Now,
                        @createdBy = userId
                    });

                    if (cartItemId > 0)
                    {
                        list.Add(new AddCartItemResponeseModel { InventoryId = item, CartItemId = cartItemId });
                        // update stock
                        UpdateStockInventory(storeId, item.ToString(), "Add Inventory");
                    }
                    //return cartItemId;
                }
                else
                {
                    throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Inventarielistan existerar inte.");
                }
            }
            return list;
        }


        public CartItemModel AddCartItemForCustomerShop(string userId, string cartId, string storeId, string inventoryCode)
        {
            // new cart item
            var sqlQry = string.Empty;
            sqlQry = GetInventoryPriceByInventoryCodeQry(inventoryCode, storeId); // get item price
            var inventory = _db.Query(sqlQry).FirstOrDefault();
            if (inventory.OutPriceIncVat > 0)
            {
                sqlQry = string.Empty;
                sqlQry = GetAddCartItemQry();
                var cartItemId = _db.ExecuteScalar<long>(sqlQry, new
                {
                    @cartId = cartId,
                    @inventoryId = inventory.Id,
                    @quantity = 1,
                    @amount = inventory.OutPriceIncVat,
                    @createdDate = DateTime.Now,
                    @createdBy = userId
                });

                CartItemModel model = new CartItemModel();

                if (cartItemId > 0)
                {
                    // update stock
                    UpdateStockInventory(storeId, Convert.ToString(inventory.Id), "Add Inventory");

                    sqlQry = string.Empty;
                    sqlQry = GetExistingCartItemForCustomerQry(cartId, Convert.ToString(inventory.Id));
                    model = _db.Query<CartItemModel>(sqlQry).FirstOrDefault();
                }
                return model;
            }
            else
            {
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Inventarielistan existerar inte.");
            }
        }

        public string AddOfflineCartItem(string userId, string cartId, string storeId, OfflineCartItemModel model)
        {
            // offline cart item

            foreach (var item in model.InventoryIds)
            {
                var sqlQry = string.Empty;
                sqlQry = GetInventoryPriceQry(item.ToString(), storeId); // get item price
                var itemPrice = _db.ExecuteScalar<decimal>(sqlQry);
                if (itemPrice > 0)
                {
                    sqlQry = string.Empty;
                    sqlQry = GetAddCartItemQry();
                    _db.Query(sqlQry, new
                    {
                        @cartId = cartId,
                        @inventoryId = item,
                        @quantity = 1,
                        @amount = itemPrice,
                        @createdDate = DateTime.Now,
                        @createdBy = userId
                    });

                    // update stock
                    UpdateStockInventory(storeId, item.ToString(), "Add Inventory");
                }
                else
                {
                    throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", string.Format("Inventarielistan {0} existerar inte.", item));
                }
            }

            Payment(userId, storeId, cartId); // payment
            return "Köpta varor är inlagda.";
        }


        private void UpdateStockInventory(string storeId, string inventoryId, string operation)
        {
            // update stock
            var sqlQry = GetAvailableStockQry(storeId, inventoryId);
            var availableStock = _db.Query<long>(sqlQry).FirstOrDefault();

            if (operation == "Add Inventory")
                availableStock -= 1;
            else if (operation == "Delete Inventory")
                availableStock += 1;

            sqlQry = string.Empty;
            sqlQry = GetUpdateStockQry(availableStock, storeId, inventoryId);
            _db.Query(sqlQry);
        }
        public void UpdateStockForRemovingCart(long units, string storeId, string inventoryId)
        {
            var sqlQuery = GetUpdateStockQry(units, storeId, inventoryId);
            _db.Execute(sqlQuery);
        }
        public void DeleteCartItem(string cartItemId, string storeId, string inventoryId)
        {
            var sqlQry = GetDeleteCartItemQry(cartItemId);
            _db.Query(sqlQry);
            UpdateStockInventory(storeId, inventoryId, "Delete Inventory");
        }

        public List<CartItemModel> GetAllCartItemList(string cartId)
        {
            var sqlQry = GetAllCartItemsForCustomerQry(cartId);
            var model = _db.Query<CartItemModel>(sqlQry).ToList();
            return model;
        }

        private static string GetDeleteCartItemQry(string cartItemId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"DELETE FROM CartItem WHERE Id = {0}", cartItemId));
            return sqlQry.ToString();
        }

        private static string GetAvailableStockQry(string storeId, string inventoryId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT NumberOfItems FROM StoreInventory WHERE StoreId = {0} AND InventoryId = {1} ", storeId, inventoryId));
            return sqlQry.ToString();
        }

        private static string GetUpdateStockQry(long units, string storeId, string inventoryId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"UPDATE StoreInventory SET NumberOfItems = {0} WHERE StoreId = {1} AND InventoryId = {2} ", units, storeId, inventoryId));
            return sqlQry.ToString();
        }

        private static string GetPaymentDoneForCartQry(string userId, string storeId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT Id,CartGeneratedDate,PaymentDone,UserId,StoreId FROM Cart WHERE CAST(CartGeneratedDate AS DATE) = '{0}' AND PaymentDone = '{1}' AND UserId = {2} AND StoreId = {3} ", DateTime.Now.Date, false, userId, storeId));
            return sqlQry.ToString();
        }

        private static string GetPreviousSessionCartPaymentQry(long cartId, string userId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"UPDATE Cart SET PaymentDone = '{0}' WHERE Id = {1} ", true, cartId));
            return sqlQry.ToString();
        }

        private static string GetPreviousSessionTimeQry(long cartId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT TOP 1 * FROM CartItem WHERE CartId = {0} ORDER BY CreatedDate DESC ", cartId));
            return sqlQry.ToString();
        }

        private static string GetGenerateCartQry()
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(@"INSERT INTO Cart (CartGeneratedDate,UserId,StoreId,CreatedDate,CreatedBy) VALUES (@cartDate,@userId,@storeId,@createdDate,@createdBy); SELECT SCOPE_IDENTITY();");
            return sqlQry.ToString();
        }

        private static string GetCartQry(string carttId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT Id FROM Cart WHERE Id = {0}; SELECT SCOPE_IDENTITY();", carttId));
            return sqlQry.ToString();
        }

        private static string GetExistingCartItemQry(string cartId, string inventoryId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT Id,CartId,InventoryId,Quantity,Amount FROM CartItem WHERE CartId = {0} AND InventoryId = {1}", cartId, inventoryId));
            return sqlQry.ToString();
        }

        private static string GetExistingCartItemForCustomerQry(string cartId, string inventoryId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT c.Id,c.CartId,c.InventoryId,c.Quantity,c.Amount,c.CreatedDate,c.CreatedBy,i.Name AS InventoryName FROM CartItem c INNER JOIN Inventory i ON c.InventoryId = i.Id WHERE CartId = {0} AND InventoryId = {1} ORDER BY id DESC", cartId, inventoryId));
            return sqlQry.ToString();
        }

        private static string GetAddCartItemQry()
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(@"INSERT INTO CartItem (CartId,InventoryId,Quantity,Amount,CreatedDate,CreatedBy) VALUES (@cartId,@inventoryId,@quantity,@amount,@createdDate,@createdBy); SELECT SCOPE_IDENTITY();");
            return sqlQry.ToString();
        }

        private static string GetInventoryPriceByInventoryCodeQry(string inventoryCode, string storeId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT Id,OutPriceIncVat FROM Inventory WHERE InventoryCode = '{0}'", inventoryCode));
            return sqlQry.ToString();
        }

        private static string GetInventoryPriceQry(string inventoryId, string storeId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT OutPriceIncVat FROM Inventory WHERE Id = {0}", inventoryId));
            return sqlQry.ToString();
        }

        private static string GetAllCartItemsQry(string cartId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT Id,CartId,InventoryId,Quantity,Amount,CreatedDate FROM CartItem WHERE CartId = {0}", cartId));
            return sqlQry.ToString();
        }

        private static string GetAllCartItemsForCustomerQry(string cartId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT c.Id,c.CartId,c.InventoryId,c.Quantity,c.Amount,c.CreatedDate,c.CreatedBy,i.Name AS InventoryName FROM CartItem c INNER JOIN Inventory i ON c.InventoryId = i.Id WHERE CartId = {0}", cartId));
            return sqlQry.ToString();
        }

        private static string GetUpdateCartItemsQry(string qty, string amount, string cartId, string inventoryId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"UPDATE CartItem SET Quantity = {0}, Amount = {1} WHERE CartId = {2} AND InventoryId = {3}", qty, amount, cartId, inventoryId));
            return sqlQry.ToString();
        }
        public bool IsCartExistForUserAndStore(long userId, long storeId)
        {
            var sqlQuery = IsCartExistForUserAndStoreQuery();
            var isCartExistForUserAndStore = _db.ExecuteScalar<int>(sqlQuery, new
            {
                @userId = userId,
                @storeId = storeId
            });
            return isCartExistForUserAndStore > 0;
        }
        private static string IsCartExistForUserAndStoreQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT COUNT(Id) FROM Cart WHERE UserId=@userId AND StoreId=@storeId;");
            return sqlQuery.ToString();
        }
        public bool RemoveCartItemsByCartId(long cartId)
        {
            var sqlQuery = RemoveCartItemsByCartIdQuery();
            _db.Execute(sqlQuery, new
            {
                @cartId = cartId
            });
            return true;
        }
        private static string RemoveCartItemsByCartIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"DELETE FROM CartItem WHERE CartId=@cartId;");
            return sqlQuery.ToString();
        }
        public Cart GetCartDetailById(long cartId)
        {
            var sqlQuery = GetCartDetailByIdQuery();
            var cartObj = _db.Query<Cart>(sqlQuery, new
            {
                @cartId = cartId
            }).FirstOrDefault();
            return cartObj;
        }
        private static string GetCartDetailByIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM Cart WHERE Id=@cartId;");
            return sqlQuery.ToString();
        }
        public CartItem GetCartItemByCartId(long cartId)
        {
            var sqlQuery = GetCartItemByCartIdQuery();
            var cartItem = _db.Query<CartItem>(sqlQuery, new
            {
                @cartId = cartId
            }).FirstOrDefault();
            return cartItem;

        }
        private static string GetCartItemByCartIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM CartItem WHERE CartId=@cartId ORDER BY CreatedDate DESC;");
            return sqlQuery.ToString();
        }
        public bool RemoveCart(long cartId)
        {
            var sqlQuery = RemoveCartQuery();
            _db.Execute(sqlQuery, new
            {
                @id = cartId
            });
            return true;
        }
        private static string RemoveCartQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"DELETE FROM Cart WHERE Id=@id;");
            return sqlQuery.ToString();
        }
        public List<TotalQuantityByCart> GetTotalQuantityByCartId(long cartId)
        {
            var sqlQuery = GetTotalQuantityByCartIdQuery();
            var totalQunatities = _db.Query<TotalQuantityByCart>(sqlQuery, new
            {
                @cartId = cartId
            }).ToList();
            return totalQunatities;
        }
        private static string GetTotalQuantityByCartIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT CartId,InventoryId,SUM(Quantity) as TotalQuantity FROM CartItem
                              WHERE CartId=@cartId
                              GROUP BY CartId,InventoryId");
            return sqlQuery.ToString();
        }
    }
}
