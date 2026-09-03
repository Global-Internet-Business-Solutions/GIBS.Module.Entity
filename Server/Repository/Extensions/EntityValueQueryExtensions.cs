using GIBS.Module.Entity.Models;
using System;
using System.Linq;

namespace GIBS.Module.Entity.Repository.Extensions
{
    /// <summary>
    /// LINQ extension methods for EntityValue queries with type-safe filtering, sorting, and searching.
    /// </summary>
    public static class EntityValueQueryExtensions
    {
        // ========================================
        // STRING / TEXT VALUE QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues by exact text match
        /// </summary>
        public static IQueryable<EntityValue> TextEquals(
            this IQueryable<EntityValue> query, int fieldId, string value)
            => query.Where(ev => ev.FieldId == fieldId && ev.TextValue == value);

        /// <summary>
        /// Filter EntityValues by text containing substring
        /// </summary>
        public static IQueryable<EntityValue> TextContains(
            this IQueryable<EntityValue> query, int fieldId, string searchText)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.TextValue != null && 
                                ev.TextValue.Contains(searchText));

        /// <summary>
        /// Filter EntityValues by text starting with
        /// </summary>
        public static IQueryable<EntityValue> TextStartsWith(
            this IQueryable<EntityValue> query, int fieldId, string prefix)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.TextValue != null && 
                                ev.TextValue.StartsWith(prefix));

        /// <summary>
        /// Filter EntityValues by text ending with
        /// </summary>
        public static IQueryable<EntityValue> TextEndsWith(
            this IQueryable<EntityValue> query, int fieldId, string suffix)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.TextValue != null && 
                                ev.TextValue.EndsWith(suffix));

        /// <summary>
        /// Filter EntityValues by text not equal
        /// </summary>
        public static IQueryable<EntityValue> TextNotEquals(
            this IQueryable<EntityValue> query, int fieldId, string value)
            => query.Where(ev => ev.FieldId == fieldId && ev.TextValue != value);


        // ========================================
        // INTEGER VALUE QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues by exact integer match
        /// </summary>
        public static IQueryable<EntityValue> IntegerEquals(
            this IQueryable<EntityValue> query, int fieldId, int value)
            => query.Where(ev => ev.FieldId == fieldId && ev.IntegerValue == value);

        /// <summary>
        /// Filter EntityValues by integer greater than
        /// </summary>
        public static IQueryable<EntityValue> IntegerGreaterThan(
            this IQueryable<EntityValue> query, int fieldId, int value)
            => query.Where(ev => ev.FieldId == fieldId && ev.IntegerValue > value);

        /// <summary>
        /// Filter EntityValues by integer greater than or equal
        /// </summary>
        public static IQueryable<EntityValue> IntegerGreaterThanOrEqual(
            this IQueryable<EntityValue> query, int fieldId, int value)
            => query.Where(ev => ev.FieldId == fieldId && ev.IntegerValue >= value);

        /// <summary>
        /// Filter EntityValues by integer less than
        /// </summary>
        public static IQueryable<EntityValue> IntegerLessThan(
            this IQueryable<EntityValue> query, int fieldId, int value)
            => query.Where(ev => ev.FieldId == fieldId && ev.IntegerValue < value);

        /// <summary>
        /// Filter EntityValues by integer less than or equal
        /// </summary>
        public static IQueryable<EntityValue> IntegerLessThanOrEqual(
            this IQueryable<EntityValue> query, int fieldId, int value)
            => query.Where(ev => ev.FieldId == fieldId && ev.IntegerValue <= value);

        /// <summary>
        /// Filter EntityValues by integer between range (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> IntegerBetween(
            this IQueryable<EntityValue> query, int fieldId, int minValue, int maxValue)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.IntegerValue >= minValue && 
                                ev.IntegerValue <= maxValue);

        /// <summary>
        /// Filter EntityValues by integer not equal
        /// </summary>
        public static IQueryable<EntityValue> IntegerNotEquals(
            this IQueryable<EntityValue> query, int fieldId, int value)
            => query.Where(ev => ev.FieldId == fieldId && ev.IntegerValue != value);


        // ========================================
        // LONG VALUE QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues by exact long match
        /// </summary>
        public static IQueryable<EntityValue> LongEquals(
            this IQueryable<EntityValue> query, int fieldId, long value)
            => query.Where(ev => ev.FieldId == fieldId && ev.LongValue == value);

        /// <summary>
        /// Filter EntityValues by long greater than
        /// </summary>
        public static IQueryable<EntityValue> LongGreaterThan(
            this IQueryable<EntityValue> query, int fieldId, long value)
            => query.Where(ev => ev.FieldId == fieldId && ev.LongValue > value);

        /// <summary>
        /// Filter EntityValues by long less than
        /// </summary>
        public static IQueryable<EntityValue> LongLessThan(
            this IQueryable<EntityValue> query, int fieldId, long value)
            => query.Where(ev => ev.FieldId == fieldId && ev.LongValue < value);

        /// <summary>
        /// Filter EntityValues by long between range (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> LongBetween(
            this IQueryable<EntityValue> query, int fieldId, long minValue, long maxValue)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.LongValue >= minValue && 
                                ev.LongValue <= maxValue);


        // ========================================
        // DECIMAL VALUE QUERIES (Prices, Measurements)
        // ========================================

        /// <summary>
        /// Filter EntityValues by exact decimal match
        /// </summary>
        public static IQueryable<EntityValue> DecimalEquals(
            this IQueryable<EntityValue> query, int fieldId, decimal value)
            => query.Where(ev => ev.FieldId == fieldId && ev.DecimalValue == value);

        /// <summary>
        /// Filter EntityValues by decimal greater than
        /// </summary>
        public static IQueryable<EntityValue> DecimalGreaterThan(
            this IQueryable<EntityValue> query, int fieldId, decimal value)
            => query.Where(ev => ev.FieldId == fieldId && ev.DecimalValue > value);

        /// <summary>
        /// Filter EntityValues by decimal greater than or equal
        /// </summary>
        public static IQueryable<EntityValue> DecimalGreaterThanOrEqual(
            this IQueryable<EntityValue> query, int fieldId, decimal value)
            => query.Where(ev => ev.FieldId == fieldId && ev.DecimalValue >= value);

        /// <summary>
        /// Filter EntityValues by decimal less than
        /// </summary>
        public static IQueryable<EntityValue> DecimalLessThan(
            this IQueryable<EntityValue> query, int fieldId, decimal value)
            => query.Where(ev => ev.FieldId == fieldId && ev.DecimalValue < value);

        /// <summary>
        /// Filter EntityValues by decimal less than or equal
        /// </summary>
        public static IQueryable<EntityValue> DecimalLessThanOrEqual(
            this IQueryable<EntityValue> query, int fieldId, decimal value)
            => query.Where(ev => ev.FieldId == fieldId && ev.DecimalValue <= value);

        /// <summary>
        /// Filter EntityValues by decimal between range (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> DecimalBetween(
            this IQueryable<EntityValue> query, int fieldId, decimal minValue, decimal maxValue)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.DecimalValue >= minValue && 
                                ev.DecimalValue <= maxValue);

        /// <summary>
        /// Filter EntityValues by decimal not equal
        /// </summary>
        public static IQueryable<EntityValue> DecimalNotEquals(
            this IQueryable<EntityValue> query, int fieldId, decimal value)
            => query.Where(ev => ev.FieldId == fieldId && ev.DecimalValue != value);


        // ========================================
        // BOOLEAN VALUE QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues by exact boolean match
        /// </summary>
        public static IQueryable<EntityValue> BooleanEquals(
            this IQueryable<EntityValue> query, int fieldId, bool value)
            => query.Where(ev => ev.FieldId == fieldId && ev.BooleanValue == value);

        /// <summary>
        /// Filter EntityValues by boolean true
        /// </summary>
        public static IQueryable<EntityValue> BooleanTrue(
            this IQueryable<EntityValue> query, int fieldId)
            => query.Where(ev => ev.FieldId == fieldId && ev.BooleanValue == true);

        /// <summary>
        /// Filter EntityValues by boolean false
        /// </summary>
        public static IQueryable<EntityValue> BooleanFalse(
            this IQueryable<EntityValue> query, int fieldId)
            => query.Where(ev => ev.FieldId == fieldId && ev.BooleanValue == false);


        // ========================================
        // DATE VALUE QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues by exact date match (date only, ignoring time)
        /// </summary>
        public static IQueryable<EntityValue> DateEquals(
            this IQueryable<EntityValue> query, int fieldId, DateTime date)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.DateValue == date.Date);

        /// <summary>
        /// Filter EntityValues by date after (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> DateAfter(
            this IQueryable<EntityValue> query, int fieldId, DateTime date)
            => query.Where(ev => ev.FieldId == fieldId && ev.DateValue >= date.Date);

        /// <summary>
        /// Filter EntityValues by date before (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> DateBefore(
            this IQueryable<EntityValue> query, int fieldId, DateTime date)
            => query.Where(ev => ev.FieldId == fieldId && ev.DateValue <= date.Date);

        /// <summary>
        /// Filter EntityValues by date between range (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> DateBetween(
            this IQueryable<EntityValue> query, int fieldId, DateTime startDate, DateTime endDate)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.DateValue >= startDate.Date &&
                                ev.DateValue <= endDate.Date);


        // ========================================
        // DATETIME VALUE QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues by exact datetime match
        /// </summary>
        public static IQueryable<EntityValue> DateTimeEquals(
            this IQueryable<EntityValue> query, int fieldId, DateTime dateTime)
            => query.Where(ev => ev.FieldId == fieldId && ev.DateTimeValue == dateTime);

        /// <summary>
        /// Filter EntityValues by datetime after (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> DateTimeAfter(
            this IQueryable<EntityValue> query, int fieldId, DateTime dateTime)
            => query.Where(ev => ev.FieldId == fieldId && ev.DateTimeValue >= dateTime);

        /// <summary>
        /// Filter EntityValues by datetime before (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> DateTimeBefore(
            this IQueryable<EntityValue> query, int fieldId, DateTime dateTime)
            => query.Where(ev => ev.FieldId == fieldId && ev.DateTimeValue <= dateTime);

        /// <summary>
        /// Filter EntityValues by datetime between range (inclusive)
        /// </summary>
        public static IQueryable<EntityValue> DateTimeBetween(
            this IQueryable<EntityValue> query, int fieldId, DateTime startDateTime, DateTime endDateTime)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.DateTimeValue >= startDateTime &&
                                ev.DateTimeValue <= endDateTime);


        // ========================================
        // GUID VALUE QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues by exact GUID match
        /// </summary>
        public static IQueryable<EntityValue> GuidEquals(
            this IQueryable<EntityValue> query, int fieldId, Guid value)
            => query.Where(ev => ev.FieldId == fieldId && ev.GuidValue == value);


        // ========================================
        // ENTITY REFERENCE QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues by referenced entity ID
        /// </summary>
        public static IQueryable<EntityValue> ReferencesEntity(
            this IQueryable<EntityValue> query, int fieldId, int referencedEntityId)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.ReferencedEntityId == referencedEntityId);

        /// <summary>
        /// Filter EntityValues by not referencing a specific entity
        /// </summary>
        public static IQueryable<EntityValue> NotReferencesEntity(
            this IQueryable<EntityValue> query, int fieldId, int referencedEntityId)
            => query.Where(ev => ev.FieldId == fieldId && 
                                ev.ReferencedEntityId != referencedEntityId);


        // ========================================
        // GENERAL / HELPER QUERIES
        // ========================================

        /// <summary>
        /// Filter EntityValues that have any value (not null/empty)
        /// </summary>
        public static IQueryable<EntityValue> HasValue(
            this IQueryable<EntityValue> query, int fieldId)
            => query.Where(ev => ev.FieldId == fieldId && 
                                (ev.TextValue != null || ev.IntegerValue.HasValue ||
                                 ev.LongValue.HasValue || ev.DecimalValue.HasValue || 
                                 ev.BooleanValue.HasValue || ev.DateValue.HasValue || 
                                 ev.DateTimeValue.HasValue || ev.GuidValue.HasValue ||
                                 ev.ReferencedEntityId.HasValue));

        /// <summary>
        /// Filter EntityValues by field ID
        /// </summary>
        public static IQueryable<EntityValue> ForField(
            this IQueryable<EntityValue> query, int fieldId)
            => query.Where(ev => ev.FieldId == fieldId);

        /// <summary>
        /// Filter EntityValues by entity ID
        /// </summary>
        public static IQueryable<EntityValue> ForEntity(
            this IQueryable<EntityValue> query, int entityId)
            => query.Where(ev => ev.EntityId == entityId);

        /// <summary>
        /// Order EntityValues by ValueIndex ascending (for multi-value fields)
        /// </summary>
        public static IQueryable<EntityValue> OrderByValueIndex(
            this IQueryable<EntityValue> query)
            => query.OrderBy(ev => ev.ValueIndex);

        /// <summary>
        /// Order EntityValues by ValueIndex descending
        /// </summary>
        public static IQueryable<EntityValue> OrderByValueIndexDescending(
            this IQueryable<EntityValue> query)
            => query.OrderByDescending(ev => ev.ValueIndex);


        // ========================================
        // ORDERING BY TYPED VALUES
        // ========================================

        /// <summary>
        /// Order EntityValues by TextValue ascending
        /// </summary>
        public static IQueryable<EntityValue> OrderByTextValue(
            this IQueryable<EntityValue> query, int fieldId, bool descending = false)
            => descending
                ? query.Where(ev => ev.FieldId == fieldId)
                        .OrderByDescending(ev => ev.TextValue)
                : query.Where(ev => ev.FieldId == fieldId)
                        .OrderBy(ev => ev.TextValue);

        /// <summary>
        /// Order EntityValues by IntegerValue ascending
        /// </summary>
        public static IQueryable<EntityValue> OrderByIntegerValue(
            this IQueryable<EntityValue> query, int fieldId, bool descending = false)
            => descending
                ? query.Where(ev => ev.FieldId == fieldId)
                        .OrderByDescending(ev => ev.IntegerValue)
                : query.Where(ev => ev.FieldId == fieldId)
                        .OrderBy(ev => ev.IntegerValue);

        /// <summary>
        /// Order EntityValues by DecimalValue ascending
        /// </summary>
        public static IQueryable<EntityValue> OrderByDecimalValue(
            this IQueryable<EntityValue> query, int fieldId, bool descending = false)
            => descending
                ? query.Where(ev => ev.FieldId == fieldId)
                        .OrderByDescending(ev => ev.DecimalValue)
                : query.Where(ev => ev.FieldId == fieldId)
                        .OrderBy(ev => ev.DecimalValue);

        /// <summary>
        /// Order EntityValues by DateValue ascending
        /// </summary>
        public static IQueryable<EntityValue> OrderByDateValue(
            this IQueryable<EntityValue> query, int fieldId, bool descending = false)
            => descending
                ? query.Where(ev => ev.FieldId == fieldId)
                        .OrderByDescending(ev => ev.DateValue)
                : query.Where(ev => ev.FieldId == fieldId)
                        .OrderBy(ev => ev.DateValue);

        /// <summary>
        /// Order EntityValues by DateTimeValue ascending
        /// </summary>
        public static IQueryable<EntityValue> OrderByDateTimeValue(
            this IQueryable<EntityValue> query, int fieldId, bool descending = false)
            => descending
                ? query.Where(ev => ev.FieldId == fieldId)
                        .OrderByDescending(ev => ev.DateTimeValue)
                : query.Where(ev => ev.FieldId == fieldId)
                        .OrderBy(ev => ev.DateTimeValue);
    }
}

