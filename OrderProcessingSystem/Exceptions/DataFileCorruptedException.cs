using System;
namespace OrderProcessingSystem.Exceptions;

// custom Exception to handle secnario of invalid json
// invalid json from deserialize throw JsonException 
// I created custom Exception so the invalid json
// will throw DataFileCorruptedException with name of invalid json file , and detials of innerException
// adding innerException keep stacktrace and messages ( details of orignal exception)
// this helps us understanding the reason of error in production
public class DataFileCorruptedException:Exception
{
    public DataFileCorruptedException(string fileName , Exception innerException) : base($"Invalid File :{fileName}",innerException)
    {

    }
}
