namespace Pronia.Utilites.Exceptions
{
    public class WrongRequestException : Exception
    {
        public WrongRequestException(string message):base(message)
        {
           
        }
        
        public WrongRequestException() : base("Sorgu yanlishdir") { }
    }
}
