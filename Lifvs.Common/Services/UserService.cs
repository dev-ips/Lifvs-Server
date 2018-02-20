using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using Lifvs.Common.Validators.Interfaces;
using log4net;
using System;
using System.Text.RegularExpressions;
using Lifvs.Common.Enums;
using System.Collections.Generic;
using Stripe;
using System.Configuration;

namespace Lifvs.Common.Services
{
    public class UserService : IUserService
    {
        private readonly ILog _log;
        private readonly IExceptionManager _exception;
        private readonly IUserRepository _userRepository;
        private readonly IAccessTokenRepository _accessTokenRepository;
        private readonly IValidatorService<UserCardDetailModel> _userCardDetailValidators;
        private readonly ICryptoGraphy _cryptoGraphy;
        private readonly IValidatorService<UserProfileViewModel> _userProfileViewValidators;
        public UserService(ILog log, IExceptionManager exception, IUserRepository userRepository, IAccessTokenRepository accessTokenRepository, IValidatorService<UserCardDetailModel> userCardDetailValidators, ICryptoGraphy cryptoGraphy, IValidatorService<UserProfileViewModel> userProfileViewValidators)
        {
            _log = log;
            _exception = exception;
            _userRepository = userRepository;
            _accessTokenRepository = accessTokenRepository;
            _userCardDetailValidators = userCardDetailValidators;
            _cryptoGraphy = cryptoGraphy;
            _userProfileViewValidators = userProfileViewValidators;
        }
        public bool AddUserCardDetails(UserCardDetailModel model)
        {
            try
            {
                _userCardDetailValidators.Validate(model);
                var errorMessage = string.Empty;
                var userCardDetailModel = new UserCardDetailModel();
                var user = _accessTokenRepository.GetUser(model.UserId.Value);

                if (user == null)
                    throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Användare finns inte.");

                //var cardExist = _userRepository.GetUserCardDetailByCardNumber(model.CardNumber, model.UserId.Value);

                //if (cardExist != null)
                //    throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Kortnummer finns redan.");


                //if (response.CvcCheck.ToLower() != "pass")
                //    throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Card is not valid.");

                //var isValidCard = CreditCardHelper.IsCardNumberValid(model.CardNumber, out errorMessage);
                //if (!isValidCard)
                //    throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", errorMessage);


                var updateCard = UpdateUserCard(model);
                if (!updateCard)
                {
                    var customers = new StripeCustomerService();

                    StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripPublishKey"]);
                    var tokenService = new StripeTokenService();

                    var token = tokenService.Create(new StripeTokenCreateOptions { Card = new StripeCreditCardOptions { Cvc = model.CVC.ToString(), Number = model.CardNumber.Replace(" ", ""), ExpirationMonth = model.ExpiredMonth, ExpirationYear = model.ExpiredYear } });

                    StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripSecretKey"]);
                    var customerId = customers.Create(new StripeCustomerCreateOptions { SourceToken = token.Id });
                    var cardType = CreditCardHelper.GetCardType(model.CardNumber);
                    StripeCardService card = new StripeCardService();
                    var cardToken = CreditCardHelper.GetCardTokens(token.StripeCard.Brand);
                    var response = card.Create(customerId.Id, new StripeCardCreateOptions { SourceToken = cardToken });

                    var userCardDetails = new UserCardDetails
                    {
                        UserId = model.UserId,
                        CardNumber = _cryptoGraphy.EncryptString(model.CardNumber),
                        CardType = cardType.ToString(),
                        ExpiredMonth = model.ExpiredMonth,
                        ExpiredYear = model.ExpiredYear,
                        CVC = model.CVC,
                        CreditCardId = customerId.Id,
                        PhoneNumber = model.PhoneNumber,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    var userCardDetailId = _userRepository.AddUserCardDetails(userCardDetails);
                }
            }
            catch (StripeException ex)
            {
                switch (ex.StripeError.ErrorType)
                {
                    case "card_error":
                        throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", ex.StripeError.Message);
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
            return true;
        }
        public bool UpdateUserCard(UserCardDetailModel model)
        {
            var userCardDetailsByUserId = _userRepository.GetUserCardDetailsByUserId(model.UserId.Value);

            if (userCardDetailsByUserId != null)
            {
                try
                {
                    var trimCard = model.CardNumber.Replace(" ", "");
                    if (trimCard.Contains("XXXXXX"))
                    {
                        userCardDetailsByUserId.ExpiredMonth = model.ExpiredMonth;
                        userCardDetailsByUserId.ExpiredYear = model.ExpiredYear;
                        userCardDetailsByUserId.CVC = model.CVC;
                        userCardDetailsByUserId.PhoneNumber = model.PhoneNumber;
                        userCardDetailsByUserId.ModifiedDate = DateTime.Now;
                        return _userRepository.UpdateUserCardData(userCardDetailsByUserId);
                    }
                    else
                    {
                        var customers = new StripeCustomerService();

                        StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripPublishKey"]);
                        var tokenService = new StripeTokenService();

                        var token = tokenService.Create(new StripeTokenCreateOptions { Card = new StripeCreditCardOptions { Cvc = model.CVC.ToString(), Number = model.CardNumber.Replace(" ", ""), ExpirationMonth = model.ExpiredMonth, ExpirationYear = model.ExpiredYear } });

                        StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripSecretKey"]);
                        var customerId = customers.Create(new StripeCustomerCreateOptions { SourceToken = token.Id });
                        var cardType = CreditCardHelper.GetCardType(model.CardNumber);
                        StripeCardService card = new StripeCardService();
                        var cardToken = CreditCardHelper.GetCardTokens(token.StripeCard.Brand);
                        var response = card.Create(customerId.Id, new StripeCardCreateOptions { SourceToken = cardToken });

                        userCardDetailsByUserId.CardNumber = _cryptoGraphy.EncryptString(model.CardNumber);
                        userCardDetailsByUserId.CardType = cardType.ToString();
                        userCardDetailsByUserId.ExpiredMonth = model.ExpiredMonth;
                        userCardDetailsByUserId.ExpiredYear = model.ExpiredYear;
                        userCardDetailsByUserId.CVC = model.CVC;
                        userCardDetailsByUserId.CreditCardId = customerId.Id;
                        userCardDetailsByUserId.PhoneNumber = model.PhoneNumber;
                        userCardDetailsByUserId.ModifiedDate = DateTime.Now;
                        return _userRepository.UpdateUserCardDetails(userCardDetailsByUserId);
                    }
                }
                catch (StripeException ex)
                {
                    switch (ex.StripeError.ErrorType)
                    {
                        case "card_error":
                            throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", ex.StripeError.Message);
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
            else
            {
                return false;
            }
            return false;
        }
    
        public bool UpdateUserCardDetails(UserCardDetailModel model)
        {
            _userCardDetailValidators.Validate(model);

            var userCardDetails = _userRepository.GetCradDetailsById(model.Id);

            if (userCardDetails == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Kortet existerar inte.");


            userCardDetails.CardNumber = _cryptoGraphy.EncryptString(model.CardNumber);
            userCardDetails.ExpiredMonth = model.ExpiredMonth;
            userCardDetails.ExpiredYear = model.ExpiredYear;
            userCardDetails.CVC = model.CVC;
            userCardDetails.PhoneNumber = model.PhoneNumber;
            userCardDetails.ModifiedDate = DateTime.Now;
            return _userRepository.UpdateUserCardDetails(userCardDetails);
        }
        public UserCardDetailModel GetUserCardDetails(long userId)
        {
            var user = _accessTokenRepository.GetUser(userId);
            var userCardDetailModel = new UserCardDetailModel();
            if (user == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Användare finns inte.");

            var userCardDetails = _userRepository.GetUserCardDetailsByUserId(userId);

            if (userCardDetails == null)
            {
                return userCardDetailModel;
            }


            var cardNumber = _cryptoGraphy.DecryptString(userCardDetails.CardNumber);
            var maskedNumber = Enums.Enums.MaskCardDigits(cardNumber);
            //cardNumber = Regex.Replace(cardNumber, @"\d{4}\ ", "xxxx ");


            userCardDetailModel.Id = userCardDetails.Id;
            userCardDetailModel.UserId = userCardDetails.UserId;
            userCardDetailModel.CardNumber = maskedNumber;
            userCardDetailModel.ExpiredMonth = userCardDetails.ExpiredMonth;
            userCardDetailModel.ExpiredYear = userCardDetails.ExpiredYear;
            userCardDetailModel.CVC = userCardDetails.CVC;
            userCardDetailModel.PhoneNumber = userCardDetails.PhoneNumber;
            userCardDetailModel.IsRegistered = true;
            return userCardDetailModel;
        }
        public List<UserViewModel> GetUsers()
        {
            var appUsers = _userRepository.GetUsers();
            return appUsers;
        }
        public UserValidCardResponse IsValidCard(long userId)
        {
            var userCardResponse = new UserValidCardResponse();

            var user = _accessTokenRepository.GetUser(userId);
            if (user == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Användare finns inte.");

            var userCardDetail = _userRepository.GetUserCardDetailsByUserId(userId);
            if (userCardDetail == null)
            {
                userCardResponse.IsValid = false;
                userCardResponse.Message = "Kortet är inte registrerat.";
                return userCardResponse;
            }

            var getCurrentYear = DateTime.Now.Year;
            var getCurrentMonth = DateTime.Now.Month;

            if ((userCardDetail.ExpiredYear == getCurrentYear && getCurrentMonth > userCardDetail.ExpiredMonth) || userCardDetail.ExpiredYear < getCurrentYear)
            {
                userCardResponse.IsValid = false;
                userCardResponse.Message = "Kortet har gått ut.";
                return userCardResponse;
            }
            userCardResponse.IsValid = true;
            userCardResponse.Message = string.Empty;
            return userCardResponse;
        }
        public UserProfileViewModel GetUserProfile(long userId)
        {
            var userProfileViewModel = new UserProfileViewModel();
            var user = _accessTokenRepository.GetUser(userId);

            userProfileViewModel.Id = user.Id;
            userProfileViewModel.Email = user.Email;
            userProfileViewModel.StreetAddress = user.StreetAddress;
            userProfileViewModel.AreaAddress = user.AreaAddress;
            userProfileViewModel.CountryId = user.CountryId;
            userProfileViewModel.PostalAddress = user.PostalAddress;
            return userProfileViewModel;
        }
        public bool ChangeProfile(long userId, UserProfileViewModel model)
        {
            //_userProfileViewValidators.Validate(model);

            model.Id = userId;
            var isDuplicateEmail = _accessTokenRepository.CheckDuplicateEmail(model.Id, model.Email);
            if (isDuplicateEmail == true)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Emailadressen är redan registrerad med en annan person.");


            var user = _accessTokenRepository.GetUser(model.Id);
            //user.Email = model.Email;
            user.AreaAddress = model.AreaAddress;
            user.StreetAddress = model.StreetAddress;
            user.PostalAddress = model.PostalAddress;
            user.CountryId = model.CountryId;

            return _userRepository.ChangeUserProfile(user);
        }
    }
}
