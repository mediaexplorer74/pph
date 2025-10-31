namespace PPH
{
    // Интерфейс для вьюшек, способных принимать нормализованный ввод
    public interface IInputConsumer
    {
        void OnInput(InputState input);
    }
}

