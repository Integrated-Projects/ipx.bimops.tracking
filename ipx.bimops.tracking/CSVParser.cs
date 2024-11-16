using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace ipx.bimops.tracking;

public class CSVParser
{
    public static IEnumerable<ModelerTrackingSchema> ProcessCsvInChunks(string filePath, int chunkSize, CancellationToken cancellationToken, int lastPosition = 0)
    {
        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            Delimiter = ",",
        };

        using var reader = new StreamReader(filePath);

        IEnumerable<ModelerTrackingSchema> records;

        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<ModelerTrackingSchemaMap>();
            records = csv.GetRecords<ModelerTrackingSchema>().Skip(lastPosition).Take(chunkSize).ToList();
        }

        return records;
    }
}

public class ModelerTrackingSchemaMap : ClassMap<ModelerTrackingSchema>
{
    public ModelerTrackingSchemaMap()
    {
        Map(m => m.id_project);
        Map(m => m.id_document);
        Map(m => m.id_user);
        Map(m => m.id_user_ip_address);
        Map(m => m.timestamp);
        Map(m => m.id_element);
        Map(m => m.type_element);
        Map(m => m.duration).TypeConverter<NullableDoubleConverter>();
        Map(m => m.action_project).TypeConverter<NullableEnumConverter<action_project>>();
        Map(m => m.action_element).TypeConverter<NullableEnumConverter<action_element>>();
    }
}
public class NullableDoubleConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null; // Return null for empty or whitespace values
        }

        // Attempt to parse as double first
        if (double.TryParse(text, out double doubleResult))
        {
            return (int)doubleResult; // Convert double to int
        }

        // Attempt to parse as int directly
        if (int.TryParse(text, out int intResult))
        {
            return intResult;
        }

        throw new TypeConverterException(this, memberMapData, text, row.Context);
    }
}

public class NullableEnumConverter<T> : DefaultTypeConverter where T : struct, Enum
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null; // Handle null or empty input for nullable enum
        }

        if (Enum.TryParse<T>(text, true, out var result))
        {
            return result;
        }

        throw new TypeConverterException(this, memberMapData, text ?? "null", row.Context);
    }

    public override string ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value == null)
        {
            return string.Empty; // Write empty string for null values
        }

        return value.ToString(); // Convert the enum value to string
    }
}
