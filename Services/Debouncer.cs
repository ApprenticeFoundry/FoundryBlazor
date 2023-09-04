/**
 * https://gist.github.com/lcnvdl/43bfdcb781d799df6b7e8e66fe3792db
 * Example:
 *
 * public void MouseMove(object sender, EventArgs event)
 * {
 *     this.debouncer.Debounce(() => this.DoSomeHeavyTask());
 * }
 *
 */
using System;
using System.Threading;

namespace FoundryBlazor.Services;


public class Debouncer : IDisposable
{
    private Thread thread;
    private volatile Action action;
    private volatile int delay = 0;
    private volatile int frequency;

    public void Debounce(Action action, int delay = 250, int frequency = 10)
    {
        this.action = action;
        this.delay = delay;
        this.frequency = frequency;

        if (this.thread == null)
        {
            this.thread = new Thread(() => this.RunThread());
            this.thread.IsBackground = true;
            this.thread.Start();
        }
    }

    private void RunThread()
    {
        while (true)
        {
            this.delay -= this.frequency;
            Thread.Sleep(this.frequency);

            if (this.delay <= 0 && this.action != null)
            {
                this.action();
                this.action = null;
            }
        }
    }

    public void Dispose()
    {
        if (this.thread != null)
        {
            this.thread.Abort();
            this.thread = null;
        }
    }
}
