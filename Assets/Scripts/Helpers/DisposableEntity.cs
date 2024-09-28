using System;
using UniRx;

namespace Game.Helpers
{
    public class DisposableEntity : IDisposable
    {
        private readonly CompositeDisposable disposables = new();
        public void Dispose()
        {
            disposables.Dispose();
        }
        ~DisposableEntity()
        {
            Dispose();
        }
        public void AddDisposable(IDisposable item)
        {
            disposables.Add(item);
        }
    }

    public static class DisposableExtentions
    {
        public static T AddTo<T>(this T disposable, DisposableEntity entity)where T: IDisposable
        {
            entity.AddDisposable(disposable);
            return disposable;
        }
    }
}