using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.ApiModels
{
    public class GeoCodeModel
    {
        public string Address { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public int? type { get; set; }
    }
    public class GeoCode
    {
        [JsonProperty("results")]
        public List<Result> Results { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }
    public class Result
    {
        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }
        [JsonProperty("address_components")]
        public List<AddressComponents> AddressComponents { get; set; }
        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }
    }
    public class Geometry
    {
        [JsonProperty("location")]
        public Location Location { get; set; }
    }
    public class Location
    {
        [JsonProperty("lng")]
        public string Longitude { get; set; }
        [JsonProperty("lat")]
        public string Latitude { get; set; }
    }
    public class AddressComponents
    {
        [JsonProperty("long_name")]
        public string LongitudeName { get; set; }
        [JsonProperty("short_name")]
        public string ShortName { get; set; }
        [JsonProperty("types")]
        public string[] Types { get; set; }
    }

}
