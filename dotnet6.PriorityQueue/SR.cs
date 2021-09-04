namespace dotnet6.PriorityQueue
{
    public static class SR
    {
        public static string ArgumentOutOfRange_NeedNonNegNum = "Index is less than zero.";
        public static string InvalidOperation_EmptyQueue = "Queue empty.";
        public static string Arg_RankMultiDimNotSupported = "Only single dimensional arrays are supported for the requested action.";
        public static string Arg_NonZeroLowerBound = "The lower bound of target array must be zero.";
        public static string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";
        public static string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
        public static string Argument_InvalidArrayType = "Target array type is not compatible with the type of items in the collection.";
        public static string InvalidOperation_EnumFailedVersion = "Collection was modified after the enumerator was instantiated.";

        public static int MaxLength =>
            // Keep in sync with `inline SIZE_T MaxArrayLength()` from gchelpers and HashHelpers.MaxPrimeArrayLength.
            0X7FFFFFC7;

    }
}