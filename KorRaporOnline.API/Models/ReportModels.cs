using System;
using System.Collections.Generic;

namespace KorRaporOnline.API.Models
{
    public enum TableType
    {
        TABLE,
        VIEW
    }

    public enum AggregationType
    {
        SUM,
        COUNT,
        AVG,
        MIN,
        MAX
    }

    public enum FilterOperator
    {
        EQUALS,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_OR_EQUAL,
        LESS_THAN_OR_EQUAL,
        LIKE,
        IN
    }

    public enum SortDirection
    {
        ASC,
        DESC
    }

    public enum ParameterType
    {
        STRING,
        NUMBER,
        DATE,
        BOOLEAN
    }
}