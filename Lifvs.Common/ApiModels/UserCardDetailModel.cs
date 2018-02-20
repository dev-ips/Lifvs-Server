namespace Lifvs.Common.ApiModels
{
    public class UserCardDetailModel
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string CardNumber { get; set; }
        public int ExpiredMonth { get; set; }
        public int ExpiredYear { get; set; }
        public int CVC { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsRegistered { get; set; }
        public string CreditCardId { get; set; }
    }
    public class UserValidCardResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
