namespace SalonAPI.Models
{
    public class BookingOverlapException: Exception
    {
        public BookingOverlapException()
        {

        }

        public BookingOverlapException(string message): base(message)
        {
                
        }
    }
}
