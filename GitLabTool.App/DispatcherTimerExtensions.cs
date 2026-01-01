using System.Windows.Threading;
using R3;

namespace GitLabTool.App;

public static class DispatcherTimerExtensions
{
    public static Observable<EventArgs> TickAsObservable(this DispatcherTimer timer)
    {
        return Observable.FromEvent<EventHandler, EventArgs>(
            h => (_, e) => h(e),
            h => timer.Tick += h,
            h => timer.Tick -= h);
    }
}