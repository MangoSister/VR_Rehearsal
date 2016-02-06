using System.Threading;

public class ThreadedJob {

    public enum JobStatus { NotStarted, Started, Succeeded, Failed};

    private readonly object _phaseLock = new object();
    private JobStatus _status = JobStatus.NotStarted;
    private object m_Handle = new object();
    private Thread m_Thread = null;


    public ThreadedJob()
    {
        m_Thread = new Thread(Run);
    }

    public JobStatus Status
    {
        get
        {
            lock(_phaseLock)
            {
                return _status;
            }
        }
        set
        {
            lock (_phaseLock)
            {
                _status = value;
            }
        }
    }

    public virtual void Start()
    {
        if (m_Thread.IsAlive)
            return;
        m_Thread.Start();
        Status = JobStatus.Started;
    }

    public virtual void Abort()
    {
        if (!m_Thread.IsAlive)
            return;
        m_Thread.Abort();
        Status = JobStatus.NotStarted;
    }

    protected virtual void ThreadFunction() { }

    //protected virtual void OnFinished() { }

    private void Run()
    {
        ThreadFunction();
        //OnFinished();
    }
}
