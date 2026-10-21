using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using OrderProcessingSystem.Exceptions;
namespace OrderProcessingSystem.Storage;

public class JsonFileStore
{
    // 1-create the path of Data folder that will store all json files
    private static readonly string DataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

// synchronous method read , write
    public List<T> Read<T>(string fileName)
    {
        string filePath = Path.Combine(DataDirectory, fileName);

        if (!File.Exists(filePath))
            return new List<T>();

        string json = File.ReadAllText(filePath);

        try
        {
            return JsonSerializer.Deserialize<List<T>>(json,
             JsonSettings.Options) ?? new List<T>();
        }
        catch (JsonException ex)
        {
            throw new DataFileCorruptedException(fileName, ex);
        }
    }

    public void Write<T>(string fileName, List<T> data)
    {
        Directory.CreateDirectory(DataDirectory);

        string filePath = Path.Combine(DataDirectory, fileName);

        string json = JsonSerializer.Serialize(data,JsonSettings.Options);

        File.WriteAllText(filePath, json);
    }

// General/Public ReadAsync Method
    public async Task<List<T>> ReadAsync<T>(string filename , CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            // return ReadCoreAsync to ensure that all updates applied to files before reading
            return await ReadCoreAsync<T>(filename, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }
// General/Public WriteAsync Method
    public async Task WriteAsync<T>(
    string fileName,
    List<T> data,
    CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            await WriteCoreAsync(fileName, data, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

//2- Private method ReadCoreAsync
// any update will apply through UpdateAsync method
    private async Task<List<T>> ReadCoreAsync<T> ( string filename , CancellationToken cancellationToken)
    {
        // create the path of file you need to read from 
        // Data/customer.json
        string filePath = Path.Combine(DataDirectory, filename);
        // 1- check if file exit 
        if (!File.Exists(filePath))
        {
            return new List<T> ();
        }
        // to read all text in file not just one customer line
        // readAllTextAsync >> read all text in file and return as string
        string json = await File.ReadAllTextAsync(filePath, cancellationToken);

// Deserialization
// convert from json (string data) to object 
        List<T>? data;
        try
        {
           data = JsonSerializer.Deserialize<List<T>>(json, JsonSettings.Options);
        }
        catch (JsonException ex)
        {
            throw new DataFileCorruptedException(filename, ex);
        }
        // Handel null data :  
        // ??   Null-coalescing operator 
        // return data if exist 
        // return new list if not
        return data ?? new List<T>();
    }

// 3- private WriteCoreAsync   
// any update will apply through UpdateAsync method
    private async Task WriteCoreAsync<T>(string fileName ,List<T> data , CancellationToken cancellationToken)
    {
        //1- find file path
        string filePath = Path.Combine(DataDirectory, fileName);
        //2- ensure that your Data Folder exist in Directory 
        // if not create one before write
        Directory.CreateDirectory(DataDirectory);
        // 3- convert object to json file using Serialize
        string json = JsonSerializer.Serialize(data, JsonSettings.Options);
        // 4- create temporary file to write instead of updating in original file direct
        string tempFilePath = Path.Combine(DataDirectory, $"{fileName}.{Guid.NewGuid():N}.tmp");
        try
        {
            //5- copy json file data into temporary file and write in this temporery file
            await File.WriteAllTextAsync(tempFilePath, json, cancellationToken);
            // check if any cancellation  
            // throw (OperationCanceledException ) , if cancellation done after completed write in temporary file
            // finally scope will handel Exception and delete the file 
            cancellationToken.ThrowIfCancellationRequested();
            // 6- when write complete move changes into the original file path
            File.Move(tempFilePath, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

    }
// Handel all asynchronization process through this method
// ensure that No Update Lost using SemaphoreSlim approch
    public async Task UpdateAsync<T>(
    string fileName,
    Action<List<T>> updateAction,
    CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            List<T> data = await ReadCoreAsync<T>(
                fileName,
                cancellationToken);

            updateAction(data);

            await WriteCoreAsync(
                fileName,
                data,
                cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
