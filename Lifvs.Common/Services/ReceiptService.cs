using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services.Interfaces;
using log4net;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lifvs.Common.Utility.Interfaces;
using System.Net.Mail;
using Lifvs.Common.Utility;
using System.Web.Configuration;

namespace Lifvs.Common.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly ILog _log;
        private readonly IReceiptRepository _receiptRepository;
        private readonly ICryptoGraphy _cryptoGraphy;
        private readonly IExceptionManager _exception;

        public ReceiptService(ILog log, IReceiptRepository receiptRepository, ICryptoGraphy cryptoGraphy, IExceptionManager exception)
        {
            _log = log;
            _receiptRepository = receiptRepository;
            _cryptoGraphy = cryptoGraphy;
        }

        public List<PurchasedHistory> GetPurchasedHistory(string userId)
        {
            return _receiptRepository.GetPurchasedHistory(userId);
        }

        public void CheckAllExpiredCarts()
        {
            var expiredCartDetails = _receiptRepository.CheckAllExpiredCarts();
            if (expiredCartDetails != null)
            {
                foreach (var item in expiredCartDetails)
                {
                    var message = string.Empty;
                    try
                    {
                        if (DateTime.Now.Subtract(item.CartGeneratedDate).TotalMinutes > CommonConstants.CartExpireTime)
                        {
                            message = Payment(item.UserId.ToString(), item.StoreId.ToString(), item.Id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(string.Format("Error while perform payment for Cart id: {0}. Error is: {1}", item.Id, ex));
                    }
                }
            }
        }

        public string Payment(string userId, string storeId, string cartId)
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

                        var message = string.Empty;
                        // Payment process
                        if (totalAmount > 0)
                        {
                            var paymentResponse = charges.Create(new StripeChargeCreateOptions { Amount = totalAmount, Description = "Lifvs purchase item payment", Currency = "SEK", CustomerId = customerId.Id });

                            if (paymentResponse.Status.Equals("succeeded", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrWhiteSpace(paymentResponse.FailureMessage))
                            {
                                var receiptId = _receiptRepository.AddReceiptItems(userId, storeId, cartId);
                                if(receiptId > 0)
                                    message = _receiptRepository.CreateTransaction(userId, paymentResponse.Status, paymentResponse.Id, paymentResponse.StripeResponse.ResponseJson, receiptId.ToString());

                                var receiptItmes = _receiptRepository.GetAllReceiptItems(receiptId.ToString());

                                if (receiptItmes != null)
                                    SendEmail(receiptItmes[0].Email, paymentResponse.Id, DateTime.Now, storeId, receiptItmes); // send receipt in mail

                                return message;
                            }
                            else
                            {
                                throw new Exception(paymentResponse.FailureMessage);
                            }
                        }
                        else
                        {
                            message = "Det finn ingen produkt i din varukorg så du behöver inte skapa ett kvitto.";
                            return message;
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

                        var message = string.Empty;
                        // Payment process
                        if (totalAmount > 0)
                        {
                            var paymentResponse = charges.Create(new StripeChargeCreateOptions { Amount = totalAmount, Description = "Lifvs purchase item payment", Currency = "SEK", CustomerId = userCardDetails.CreditCardId });

                            if (paymentResponse.Status.Equals("succeeded", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrWhiteSpace(paymentResponse.FailureMessage))
                            {
                                var receiptId = _receiptRepository.AddReceiptItems(userId, storeId, cartId);
                                if(receiptId > 0)
                                    message = _receiptRepository.CreateTransaction(userId, paymentResponse.Status, paymentResponse.Id, paymentResponse.StripeResponse.ResponseJson, receiptId.ToString());

                                var receiptItmes = _receiptRepository.GetAllReceiptItems(receiptId.ToString());

                                if (receiptItmes != null)
                                    SendEmail(receiptItmes[0].Email, paymentResponse.Id, DateTime.Now, storeId, receiptItmes); // send receipt in mail

                                return message;
                            }
                            else
                            {
                                throw new Exception(paymentResponse.FailureMessage);
                            }
                        }
                        else
                        {
                            _receiptRepository.AddReceiptItems(userId, storeId, cartId);
                            message = "Det finn ingen produkt i din varukorg så du behöver inte skapa ett kvitto.";
                            return message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool SendReceipt(long receiptId, long userId)
        {
            var isReceiptExist = _receiptRepository.IsReceiptExist(receiptId, userId);
            if (!isReceiptExist)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Det finns iget kvitto för denna användare.");

            var receiptItmes = _receiptRepository.GetAllReceiptItems(receiptId.ToString());
            var receiptObj = _receiptRepository.GetReceiptById(receiptId);
            if (receiptObj == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Kvittot existerar inte.");

            var paymentDetail = _receiptRepository.GetTransactionIdByReceiptId(receiptId);
            if (paymentDetail == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Det finns inga transaktioner för detta kvitto.");

            if (receiptItmes == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Det finns inga produkter på detta kvitto.");

            SendEmail(receiptItmes[0].Email, paymentDetail, DateTime.Now, receiptObj.StoreId.ToString(), receiptItmes); // send receipt in mail
            return true;
        }
        public void SendEmail(string toMail, string paymentId, DateTime receiptDate, string storeId, List<ReceiptItemModel> receiptItmes)
        {
            try
            {
                //var isZoho = false;

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
                //if (isZoho)
                //    smtp.Host = "smtp.zoho.com";
                //else
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
            htmlBodyContent += "<div style='padding-right: 15px;padding-left: 15px;margin-right: auto;margin-left: auto;background-color:white;'>"; // container
            htmlBodyContent += "<div style='margin-right: -15px;margin-left: -15px; '>"; // row
            htmlBodyContent += "<div style='min-height: 20px;padding: 19px;margin-bottom: 20px;background-color: white;border: 1px solid #e3e3e3;border-radius: 4px;-webkit-box-shadow: inset 0 1px 1px rgba(0, 0, 0, .05);box-shadow: inset 0 1px 1px rgba(0, 0, 0, .05);position: relative;min-height: 1px;padding-right: 15px;padding-left: 15px; float:left;width:50%; margin-left: 8.33333333%; margin-left: 25%;'>"; //well col-xs-10 col-sm-10 col-md-6 col-xs-offset-1 col-sm-offset-1 col-md-offset-3
            htmlBodyContent += "<div style='margin-right: -15px;margin-left: -15px; '>"; // row
            htmlBodyContent += "<div style='width: 50%;float:left;position: relative;min-height: 1px;padding-right: 15px;padding-left: 15px;'>"; //col-xs-6 col-sm-6 col-md-6

            htmlBodyContent += "</div>";

            htmlBodyContent += "<div style='width: 95%;float:left;position: relative;min-height: 1px;padding-right: 15px;padding-left: 15px;text-align:right;'>"; //col-xs-6 col-sm-6 col-md-6 text-right
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
            //htmlBodyContent += "<thead style='background-color: #e8e8e8;'>";
            //htmlBodyContent += "<tr style='background-color: #e8e8e8;'>";
            //htmlBodyContent += "<th style='text-align:left;'>Inventory</th>";
            //htmlBodyContent += "<th style='text-align:left;'>Price</th>";
            //htmlBodyContent += "</tr>";
            //htmlBodyContent += "</thead>";
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
            //htmlBodyContent = "<div>";
            //htmlBodyContent += "<div style='background-color: lightgrey;padding-left: 10px; padding-top: 5px; padding-bottom: 5px;'>";
            //htmlBodyContent += "Hi,";
            //htmlBodyContent += "<br /><br /> Thank you for shopping!";
            //htmlBodyContent += "<br /><br /> Below is a receipt of your shopping from LIFVS store.";
            //htmlBodyContent += string.Format("<br /><br /> Your payment id is <b>{0}</b>.", paymentId);
            //htmlBodyContent += string.Format("<br /><br /> <b> Butik {0}, {1} </b><br /><br />", storeId, receiptDate.ToShortDateString());

            //htmlBodyContent += "<div>";
            //htmlBodyContent += "<center>";
            //htmlBodyContent += "<table style='border-spacing: 0px;border-top-color: orange;width: 70%;line-height: 2.5;max-width: 100%;margin-bottom: 1rem;box-sizing: border-box;border-collapse: collapse;background-color: transparent;'>";
            //foreach (var item in receiptItems)
            //{
            //    htmlBodyContent += "<tr>";
            //    htmlBodyContent += "<td style='border-bottom: 1px solid #9c7676; padding-right: 20px;border-top: 1px solid #9c7676;'>" + item.Inventory + "";
            //    htmlBodyContent += "</td>";

            //    htmlBodyContent += "<td style='border-bottom: 1px solid #9c7676; padding-right: 20px;border-top: 1px solid #9c7676;'>" + Convert.ToInt32(item.Amount) + "";
            //    htmlBodyContent += "</td>";
            //    htmlBodyContent += "</tr>";

            //    amount += Convert.ToInt32(item.Amount);
            //}
            //htmlBodyContent += "<tr>";
            //htmlBodyContent += "<td><b> Total </b></td>";
            //htmlBodyContent += "<td> <b>" + amount + " SEK </b></td>";
            //htmlBodyContent += "</tr>";
            //htmlBodyContent += "</table>";
            //htmlBodyContent += "</center>";
            //htmlBodyContent += "</div>";

            //htmlBodyContent += "<br /><br />Best, <br />LIFVS Team";
            //htmlBodyContent += "</div>";
            //htmlBodyContent += "</div>";
            return htmlBodyContent;
        }

        public List<ReceiptViewModel> GetStoreReceipts(int storeId, int offSet, int rows, string filters, string sidx, string sord)
        {
            string whereCondition = GetSearchQueryOfStoreReceipts(filters, sidx, sord);
            string orderBy = GetOrderBy(sidx, sord);
            return _receiptRepository.GetStoreReceipts(storeId, whereCondition, orderBy);
        }
        public ReceiptHistoryDetailModel GetReceiptHistoryDetail(long userId, long receiptId)
        {
            var isReceiptExist = _receiptRepository.IsReceiptExist(receiptId, userId);
            if (!isReceiptExist)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Det finns iget kvitto för denna användare.");

            return _receiptRepository.GetReceiptHistoryDetail(receiptId);
        }
        private static string GetSearchQueryOfStoreReceipts(string filters, string sidx, string sord)
        {
            GetFilters filterData = JsonConvert.DeserializeObject<GetFilters>(filters);
            string whereCondition = string.Empty;
            if (filterData != null)
            {
                if (filterData.rules.Count > 0)
                {
                    foreach (var item in filterData.rules)
                    {
                        var fieldText = string.Empty;
                        switch (item.field)
                        {
                            case "Id":
                                fieldText = "r.Id";
                                break;
                            case "ReceiptDate":
                                fieldText = "r.ReceiptDate";
                                break;
                            case "Amount":
                                fieldText = "r.Amount";
                                break;
                            case "Vat1":
                                fieldText = "r.Vat1";
                                break;
                            case "Vat2":
                                fieldText = "r.Vat2";
                                break;
                            case "Email":
                                fieldText = "U.Email";
                                break;
                            case "TotalArticles":
                                fieldText = "r.TotalArticles";
                                break;
                        }
                        switch (item.op)
                        {
                            case "ne":
                                if (!string.IsNullOrEmpty(whereCondition))
                                    whereCondition = whereCondition + " AND" + fieldText + " <> '" + item.data + "'";
                                else
                                    whereCondition = " WHERE " + fieldText + "<> '" + item.data + "'";
                                break;
                            case "eq":
                                if (!string.IsNullOrEmpty(whereCondition))
                                    whereCondition = whereCondition + " AND" + fieldText + "= '" + item.data + "'";
                                else
                                    whereCondition = " WHERE " + fieldText + "='" + item.data + "'";
                                break;
                            case "bw":
                                if (!string.IsNullOrEmpty(whereCondition))
                                    whereCondition = whereCondition + " and" + fieldText + " like '" + item.data + "%'";
                                else
                                    whereCondition = " WHERE " + fieldText + " like '" + item.data + "%'";
                                break;
                            case "bn":
                                if (!string.IsNullOrEmpty(whereCondition))
                                    whereCondition = whereCondition + " AND " + fieldText + " not like '" + item.data + "%'";
                                else
                                    whereCondition = " WHERE " + fieldText + " not like '" + item.data + "%'";
                                break;
                            case "ew":
                                if (!string.IsNullOrEmpty(whereCondition))
                                    whereCondition = whereCondition + " and" + fieldText + " like '%" + item.data + "'";
                                else
                                    whereCondition = " WHERE " + fieldText + " like '%" + item.data + "'";
                                break;
                            case "en":
                                if (!string.IsNullOrEmpty(whereCondition))
                                    whereCondition = whereCondition + " AND" + fieldText + " not like '%" + item.data + "'";
                                else
                                    whereCondition = " WHERE " + fieldText + " not like '%" + item.data + "'";
                                break;
                            case "cn":
                                //if (!string.IsNullOrEmpty(whereCondition) && fieldText != " StartDate")
                                //    whereCondition = whereCondition + " and" + fieldText;
                                if (fieldText == "ReceiptDate" && string.IsNullOrEmpty(whereCondition))
                                    whereCondition = " WHERE " + " CAST(" + fieldText + " As date)" + " = '" + item.data + "'";
                                else if (!string.IsNullOrEmpty(whereCondition) && fieldText == "ReceiptDate")
                                    whereCondition = whereCondition + " AND " + " CAST(" + fieldText + " As date)" + " = '" + item.data + "'";
                                //else if (!string.IsNullOrEmpty(whereCondition) && fieldText != " EndDate")
                                //    whereCondition = whereCondition + " and" + fieldText;
                                else if (!string.IsNullOrEmpty(whereCondition))
                                    whereCondition = whereCondition + " AND " + fieldText + " like '" + item.data + "%'";
                                else
                                    whereCondition = " WHERE " + fieldText + " like '" + item.data + "%'";
                                break;
                            case "nc":
                                if (!string.IsNullOrEmpty(whereCondition))
                                    whereCondition = whereCondition + " AND " + fieldText + " not like '%" + item.data + "%'";
                                else
                                    whereCondition = " WHERE " + fieldText + " not like '%" + item.data + "%'";
                                break;
                        }
                    }
                }
            }
            return whereCondition;
        }
        private static string GetOrderBy(string sidx, string sord)
        {
            string orderBy = string.Empty;
            if (!String.IsNullOrEmpty(sidx) && !String.IsNullOrEmpty(sord))
                orderBy = sidx + " " + sord;
            return orderBy;
        }

    }
}
