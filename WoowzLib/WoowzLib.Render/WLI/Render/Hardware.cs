namespace WLI_Render;

// Рендер векторами/полигонами
public interface Hardware : WLI.Render, WLI.Engine{
    void FrameStart();
    void FrameStop();
    
    // TODO, надо будет реализовать общие функции, когда буду делать ещё другое графическое ядро, а то не дело
}