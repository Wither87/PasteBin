using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using PasteBinASP.Constants;
using PasteBinASP.ObjectEntities;
using System.Net;

namespace PasteBinASP.ObjectStorages.Impl;

public class PasteObjectStorage : IPasteObjectStorage
{
    private const string BucketName = "pastebin";
    private const string ContentType = "application/text";
    #region Шаблоны для форматирования строк
    private const string ObjectFoundFormat = "Объект {0} найден.";
    private const string ObjectDeletedFormat = "Объект {0} удалён.";
    private const string ObjectAddedFormat = "Объект {0} добавлен.";
    private const string ObjectRetrievedFormat = "Объект {0} получен.";
    private const string BucketExistsFormat = "Объект {0} существует.";
    #endregion
    private readonly ILogger<PasteObjectStorage> _logger;
    private readonly IMinioClient _minioClient;

    public PasteObjectStorage(ILogger<PasteObjectStorage> logger, IMinioClient minioClient)
    {
        _logger = logger;
        _minioClient = minioClient;
    }

    public async Task AddAsync(string key, string value)
    {
        try
        {
            await BucketExists();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(value);
            writer.Flush();
            stream.Position = 0;

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(ContentType);
            await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
            _logger.LogInformation(string.Format(ObjectAddedFormat, key));
        }
        catch (MinioException e)
        {
            _logger.LogError(e.Message);
        }
    }

    public async Task DeleteAsync(params string[] keys)
    {
        foreach (var key in keys)
        {
            try
            {
                await BucketExists();

                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(key);
                await _minioClient.StatObjectAsync(statObjectArgs);
                _logger.LogInformation(string.Format(ObjectFoundFormat,key));

                var putObjectArgs = new RemoveObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(key);
                await _minioClient.RemoveObjectAsync(putObjectArgs).ConfigureAwait(false);
                _logger.LogInformation(string.Format(ObjectDeletedFormat, key));
            }
            catch (MinioException e)
            {
                _logger.LogError(e.Message);
            }
        }
    }

    public async Task<PasteObjectEntity> GetAsync(string key)
    {
        var pasteObjectEntity = new PasteObjectEntity();
        try
        {
            await BucketExists();

            var statObjectArgs = new StatObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key);
            await _minioClient.StatObjectAsync(statObjectArgs);
            _logger.LogInformation(string.Format(ObjectFoundFormat, key));

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key)
                .WithCallbackStream((stream) =>
                {
                    StreamReader reader = new StreamReader(stream);
                    pasteObjectEntity.Text = reader.ReadToEnd();
                });

            await _minioClient.GetObjectAsync(getObjectArgs).ConfigureAwait(false);
            pasteObjectEntity.StatusCode = HttpStatusCode.OK;
            _logger.LogInformation(string.Format(ObjectRetrievedFormat, key));
        }
        catch (ObjectNotFoundException)
        {
            pasteObjectEntity.StatusCode = HttpStatusCode.NotFound;
            pasteObjectEntity.ErrorMessage = ErrorMessages.NotFound;
        }
        catch (MinioException e)
        {
            pasteObjectEntity.StatusCode = HttpStatusCode.InternalServerError;
            _logger.LogError(e.Message);
        }

        return pasteObjectEntity;
    }

    private async Task BucketExists()
    {
        try
        {
            var beArgs = new BucketExistsArgs()
                    .WithBucket(BucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(BucketName);
                await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            }
            _logger.LogInformation(string.Format(BucketExistsFormat, BucketName));
        }
        catch (MinioException e)
        {
            _logger.LogError(e.Message);
        }
    }
}
