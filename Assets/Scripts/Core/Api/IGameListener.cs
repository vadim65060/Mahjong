namespace Core.Api
{
    public interface IService {}

    public interface IGameListener : IService {}

    public interface IUpdateListener : IGameListener
    {
        void OnUpdate();
    }

    public interface IGamePauseListener : IGameListener
    {
        void OnPause();
    }

    public interface IGameResumeListener : IGameListener
    {
        void OnResume();
    }
}