namespace WLO;

public class ExceptionWL : Exception{
    public const string DefaultNoMessage = "Не указана ошибка! Указано (null)!";
    
    public ExceptionWL(                                           ) : base(DefaultNoMessage                            ){}
    public ExceptionWL(string? Message                            ) : base(Message ?? DefaultNoMessage                 ){}
    public ExceptionWL(string? Message, Exception? ParentException) : base(Message ?? DefaultNoMessage, ParentException){}
}