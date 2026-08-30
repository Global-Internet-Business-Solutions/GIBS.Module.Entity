namespace GIBS.Module.Entity.Enums
{
    /// <summary>
    /// Defines the data type for entity field storage.
    /// Determines which typed column in EntityValue will be used.
    /// </summary>
    public enum EntityDataType
    {
        String = 1,
        Integer = 2,
        Long = 3,
        Decimal = 4,
        Boolean = 5,
        Date = 6,
        DateTime = 7,
        Guid = 8,
        Json = 9
    }
}
