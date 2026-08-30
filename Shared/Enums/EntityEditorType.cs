namespace GIBS.Module.Entity.Enums
{
    /// <summary>
    /// Defines how the field should be rendered in the UI.
    /// Separate from DataType to allow different presentations of the same data.
    /// </summary>
    public enum EntityEditorType
    {
        TextBox = 1,
        TextArea = 2,
        RichText = 3,
        Number = 4,
        Currency = 5,
        Checkbox = 6,
        Toggle = 7,
        DatePicker = 8,
        DateTimePicker = 9,
        Dropdown = 10,
        RadioList = 11,
        CheckboxList = 12,
        MultiSelect = 13,
        EntityLookup = 14,
        FileUpload = 15,
        ImageUpload = 16,
        HtmlEditor = 17
    }
}
