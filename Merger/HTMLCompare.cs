namespace Merger
{
    public class HtmlCompare 
    {
        private Merger _mymerger;


        public string GetMergedVersion(string original, string modified)
        {
            _mymerger = new Merger(original, modified);
            return _mymerger.Merge();
        }

        public int BlocksSame => _mymerger.BlocksSame;

        public int BlocksInserted => _mymerger.BlocksInserted;

        public int BlocksDeleted => _mymerger.BlocksDeleted;

        public void Reset()
        {
            _mymerger = null;
        }

    }
}
