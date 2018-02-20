namespace Lifvs.Common.DataModels
{
    public class StoreRatings
    {
        public virtual long Id { get; set; }
        public virtual long StoreId { get; set; }
        public virtual long UserId { get; set; }
        public virtual decimal Rating { get; set; }
    }
}
