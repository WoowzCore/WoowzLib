namespace WLO{

    /// <summary>
    /// Ошибка библиотеки WoowzLib
    /// </summary>
    public class WLException : Exception{
        public WLException(                               ) : base(              ){}
        public WLException(string Message                 ) : base(Message       ){}
        public WLException(string Message, Exception Inner) : base(Message, Inner){}
    }
}