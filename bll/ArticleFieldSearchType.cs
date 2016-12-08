namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Тип поиска по полям
    /// </summary>
    public enum ArticleFieldSearchType
    {
        None = 0,
        FullText,
        Text,
        DateRange,
        TimeRange,
        NumericRange,
        Boolean,
        M2MRelation,
        O2MRelation,
        M2ORelation,
        Classifier,
        DateTimeRange,
        StringEnum,
        Identifier
    }
}
