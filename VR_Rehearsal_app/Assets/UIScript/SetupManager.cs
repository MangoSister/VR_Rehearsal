public class SetupManager {

    bShowcaseManager _bShowcaseMgr;

    public bShowcaseManager BShowcaseMgr  
    {
        get
        {
            return _bShowcaseMgr;
        }
    }

    public SetupManager()
    {
         Start();
    }

    public void Start () {
        _bShowcaseMgr = new bShowcaseManager();
      //  _bShowcaseMgr.Start();
    }

}
