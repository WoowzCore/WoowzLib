namespace WLO;

public class ExceptionWL : Exception{
    public ExceptionWL(                                           ) : base("Не указана ошибка!"    ){}
    public ExceptionWL(string? Message                            ) : base(Message                 ){}
    public ExceptionWL(string? Message, Exception? ParentException) : base(Message, ParentException){}
}