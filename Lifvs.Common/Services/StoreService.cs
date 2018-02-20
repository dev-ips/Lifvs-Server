using iTextSharp.text;
using iTextSharp.text.pdf;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services.Interfaces;
using log4net;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lifvs.Common.Services
{
    public class StoreService : IStoreService
    {
        private readonly ILog _log;
        private readonly IExceptionManager _exception;
        private readonly IStoreRepository _storeRepository;
        private readonly ICartService _cartService;
        public StoreService(ILog log, IExceptionManager exception, IStoreRepository storeRepository, ICartService cartService)
        {
            _log = log;
            _exception = exception;
            _storeRepository = storeRepository;
            _cartService = cartService;
        }

        public long GetStoreCode(string userId, string storeId)
        {
            var storeCode = _storeRepository.GetStoreCode(userId, storeId);
            if (storeCode > 0)
            {
                return storeCode;
            }
            else
            {
                storeCode = Convert.ToInt64(CommonConstants.GenerateRandomCode());
                if (_storeRepository.SaveStoreCode(userId, storeId, storeCode.ToString()) > 0)
                {
                    return storeCode;
                }
                else
                {
                    throw _exception.ThrowException(System.Net.HttpStatusCode.InternalServerError, "", "Fel vid hämtande av butikskod.");
                }
            }
        }

        public List<StoresResponseModel> GetStores()
        {
            var storeModelList = new List<StoresResponseModel>();
            var allStores = _storeRepository.GetStores();
            storeModelList = allStores.ConvertAll(x => new StoresResponseModel()
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone,
                Address = x.Address,
                City = x.City,
                PostalCode = x.PostalCode,
                QRCode = x.QRCode,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                InternalNumber = x.InternalNumber,
                Rating = x.Rating,
                StoreNumber = x.StoreNumber,
                TotalRatings = x.TotalRatings
                //InventoryIds = _storeRepository.GetStoreInventories(x.Id).ToArray()
            });
            return storeModelList;
        }
        public async Task<long> AddStoreDetails(AddStoreModel model)
        {
            var tmpAddress = model.Address;
            if (!tmpAddress.ToLower().Contains(model.City.ToLower()))
                tmpAddress += string.Format(" {0}", model.City);
            if (!tmpAddress.ToLower().Contains(model.PostalCode.ToLower()))
                tmpAddress += string.Format(" {0}", model.PostalCode);

            var latAndLong = await GetLongAndLatFromAddress(tmpAddress);
            if (latAndLong != null)
            {
                model.Latitude = latAndLong.Lattitude;
                model.Longitude = latAndLong.Longitude;
                var storeObj = new Stores
                {
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    Rating = model.Rating,
                    StoreNumber = model.StoreNumber,
                    SupervisorName = model.SupervisorName,
                    QRCode = model.QRCode,
                    CreatedDate = DateTime.Now
                };
                var storeId = _storeRepository.AddStoreDetails(storeObj);
                return storeId;
            }
            else
            {
                throw new Exception("Adressen hittades inte.");
            }
        }
        public AddStoreModel GetStoreDetailById(long storeId)
        {
            var storeDetail = _storeRepository.GetStoreDetailById(storeId);
            if (storeDetail == null)
                throw new Exception("Butiken finns inte.");
            var storeModel = new AddStoreModel
            {
                Id = storeDetail.Id,
                Name = storeDetail.Name,
                Email = storeDetail.Email,
                Phone = storeDetail.Phone,
                Address = storeDetail.Address,
                City = storeDetail.City,
                PostalCode = storeDetail.PostalCode,
                Rating = storeDetail.Rating,
                StoreNumber = storeDetail.StoreNumber,
                SupervisorName = storeDetail.SupervisorName,
                QRCode = storeDetail.QRCode
            };
            return storeModel;
        }
        public async Task<bool> UpdateStoreDetails(AddStoreModel model)
        {
            var storeDetail = _storeRepository.GetStoreDetailById(model.Id);
            if (storeDetail == null)
                throw new Exception("Butiken finns inte.");


            var tmpAddress = model.Address;
            if (!tmpAddress.ToLower().Contains(model.City.ToLower()))
                tmpAddress += string.Format(" {0}", model.City);
            if (!tmpAddress.ToLower().Contains(model.PostalCode.ToLower()))
                tmpAddress += string.Format(" {0}", model.PostalCode);

            var latAndLong = await GetLongAndLatFromAddress(tmpAddress);
            if (latAndLong == null)
                throw new Exception("Adressen hittades inte.");

            storeDetail.Name = model.Name;
            storeDetail.Email = model.Email;
            storeDetail.Phone = model.Phone;
            storeDetail.Address = model.Address;
            storeDetail.City = model.City;
            storeDetail.PostalCode = model.PostalCode;
            storeDetail.Latitude = latAndLong.Lattitude;
            storeDetail.Longitude = latAndLong.Longitude;
            storeDetail.Rating = model.Rating;
            storeDetail.StoreNumber = model.StoreNumber;
            storeDetail.SupervisorName = model.SupervisorName;
            storeDetail.QRCode = model.QRCode;

            return _storeRepository.UpdateStoreDetails(storeDetail);
        }
        private async Task<GeoCodeModel> GetLongAndLatFromAddress(string address)
        {
            var geoCode = new GeoCode();
            var geoCodeModel = new GeoCode();
            try
            {
                var lastAddressWord = string.Empty;
                var lastFormattedWord = string.Empty;

                var geoCodeReturnModel = new GeoCodeModel();
                using (var client = new HttpClient())
                {
                    var urlGeneral = "https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}";
                    var googleApiKey = ConfigurationManager.AppSettings["GoogleApiKey"];
                    var uri = new Uri(string.Format(urlGeneral, address, googleApiKey));
                    HttpResponseMessage response = await client.GetAsync(uri);

                    var content = await response.Content.ReadAsStringAsync();
                    geoCode = JsonConvert.DeserializeObject<GeoCode>(content);
                    if (geoCode.Status == "OK")
                    {
                        foreach (var result in geoCode.Results)
                        {
                            var streetNumber = string.Empty;
                            var postCode = string.Empty;
                            var route = string.Empty;

                            foreach (var type in result.AddressComponents)
                            {
                                var types = type.Types;
                                if (types.Contains("street_number"))
                                    streetNumber = type.LongitudeName.ToLower();
                                else if (types.Contains("postal_code"))
                                    postCode = type.LongitudeName.ToLower().Replace(" ", "");
                                else if (types.Contains("route"))
                                    route = type.LongitudeName.ToLower();
                            }

                            var isStreetNumberMatch = false;
                            var isPostCodeMatch = false;

                            if (!string.IsNullOrEmpty(streetNumber) && address.ToLower().Contains(streetNumber))
                                isStreetNumberMatch = true;

                            if (!string.IsNullOrEmpty(route) && address.ToLower().Contains(route))
                                isStreetNumberMatch = true;

                            if (!string.IsNullOrEmpty(postCode) && address.ToLower().Contains(postCode))
                                isPostCodeMatch = true;

                            if (isPostCodeMatch || isStreetNumberMatch)
                            {
                                geoCodeReturnModel.Lattitude = result.Geometry.Location.Latitude;
                                geoCodeReturnModel.Longitude = result.Geometry.Location.Longitude;
                                break;
                            }
                        }
                        return geoCodeReturnModel;
                    }
                    else
                    {
                        _log.WarnFormat("Not found location for {0} ", address);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in GoogleService.GetGeoLocation {0}", ex.Message);
                throw;
            }
        }

        public List<StoreViewModel> GetAllStores()
        {
            var stores = _storeRepository.GetAllStores();
            return stores;
        }

        public List<StoreViewModel> GetNearByStores(StoreInputModel model)
        {
            var storeList = new List<StoreViewModel>();
            if (string.IsNullOrEmpty(model.Latitude) && string.IsNullOrEmpty(model.Longitude))
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

            if (string.IsNullOrEmpty(model.Latitude) || string.IsNullOrEmpty(model.Longitude))
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

            var nearByStores = _storeRepository.GetNearByStores(model.Latitude, model.Longitude);
            storeList.AddRange(nearByStores.Select(x => new StoreViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone,
                Address = x.Address,
                City = x.City,
                PostalCode = x.PostalCode,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Rating = x.Rating,
                StoreNumber = x.StoreNumber,
                Distance = x.Distance,
                TotalRatings = x.TotalRatings
            }));
            return storeList;
        }
        public List<StoreViewModel> GetSearchedStores(StoreInputModel model)
        {
            if (string.IsNullOrEmpty(model.SearchFied))
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");
            var searchedFields = model.SearchFied.Replace(",", " ");
            var searchedStores = _storeRepository.GetSearchedStores(searchedFields);
            var storeList = new List<StoreViewModel>();
            Random random = new Random();
            storeList = searchedStores.ConvertAll(x => new StoreViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone,
                Address = x.Address,
                City = x.City,
                PostalCode = x.PostalCode,
                QRCode = x.QRCode,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Rating = x.Rating,
                StoreNumber = x.StoreNumber,
                SupervisorName = x.SupervisorName,
                TotalRatings = x.TotalRatings
            });
            return storeList;
        }

        public void DeleteStoreDetails(long storeId)
        {
            _storeRepository.DeleteStoreDetails(storeId);
        }
        public List<StoreViewModel> GetRecentlyVisitedStores()
        {
            var storeList = new List<StoreViewModel>();
            Random random = new Random();
            var recentlyVisitedStores = _storeRepository.GetRecentlyVisitedStores();
            storeList.AddRange(recentlyVisitedStores.Select(x => new StoreViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone,
                Address = x.Address,
                City = x.City,
                PostalCode = x.PostalCode,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Rating = x.Rating,
                StoreNumber = x.StoreNumber,
                TotalRatings = x.TotalRatings
            }));
            return storeList;
        }
        public ScanStoreResponseModel ScanStoreCode(ScanStoreModel model)
        {
            var scanStoreResponseModel = new ScanStoreResponseModel();
            var store = _storeRepository.GetStoreByQRCode(model.QRCode, model.StoreId);
            if (store == null)
            {
                scanStoreResponseModel.Massage = "Scan giltiga QR kod för att börja shoppa.";
                return scanStoreResponseModel;
                //throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "QR code does not exist.");
            }
            var cartId = _cartService.GenerateCart(Convert.ToString(model.UserId), Convert.ToString(store.Id));
            scanStoreResponseModel.CartId = cartId;
            scanStoreResponseModel.IsValid = true;
            scanStoreResponseModel.Massage = "Butiken är scannad och registrerad.";
            return scanStoreResponseModel;
        }
        public List<StoreDropDownModel> GetStoresDropDown()
        {
            return _storeRepository.GetStoresDropDown();
        }
        public bool CreateAndSaveStoreCode(string qrCodeDigit, string storeName)
        {
            var inputCode = qrCodeDigit;
            using (MemoryStream ms = new MemoryStream())
            {

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(inputCode, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap bitMap = qrCode.GetGraphic(20))
                {
                    bitMap.Save(ms, ImageFormat.Png);
                    System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                    img.Save(HttpContext.Current.Server.MapPath("~/QRCodes/") + inputCode + ".jpeg", ImageFormat.Jpeg);
                    //ViewBag.QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                    Document doc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                    string pdfFilePath = HttpContext.Current.Server.MapPath("~/PdfFiles");
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfFilePath + "/" + inputCode + ".pdf", FileMode.Create));
                    doc.Open();
                    try

                    {

                        Paragraph paragraph = new Paragraph(storeName);

                        string imageURL = HttpContext.Current.Server.MapPath("~/QRCodes") + "\\" + inputCode + ".jpeg";

                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);

                        //Resize image depend upon your need

                        jpg.ScaleToFit(140f, 120f);

                        //Give space before image

                        jpg.SpacingBefore = 10f;

                        //Give some space after the image

                        jpg.SpacingAfter = 1f;

                        jpg.Alignment = Element.ALIGN_LEFT;


                        doc.Add(paragraph);

                        doc.Add(jpg);
                        //doc.Close();
                        //TempData["FileName"] = model.QRCode;
                        return true;
                    }

                    catch (Exception ex)

                    {
                        throw new Exception(ex.Message);
                    }

                    finally

                    {
                        doc.Close();
                    }
                }
            }
        }
        public bool RateStore(StoreRateModel model)
        {
            if (model.Rating <= 0)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltigt värde för butik betyg.");

            var store = _storeRepository.GetStoreDetailById(model.StoreId);

            if (store == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Butiken finns inte.");

            var storeRating = new StoreRatings
            {
                StoreId = model.StoreId,
                UserId = model.UserId,
                Rating = model.Rating
            };

            _storeRepository.AddStoreRatings(storeRating);
            var averageRatings = _storeRepository.GetStoreAverageRatings(model.StoreId);

            store.Rating = averageRatings;
            return _storeRepository.UpdateStoreRating(store);
        }
    }
}
