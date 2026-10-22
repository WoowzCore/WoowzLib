using System.Text;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace WLO;

public enum YouTubePrivacy{
    Public,
    Link,
    Unpublic
}

public enum YouTubeFailReason{
    None,
    QuotaExceeded,
    UploadLimitExceeded,
    Forbidden,
    NotFound,
    AuthenticationError,
    NetworkError,
    Cancelled,
    Other
}

public class YouTubeOperationResult<T>{
    public bool              Success{ get; init; }
    public T?                Value{ get; init; }
    public YouTubeFailReason FailReason{ get; init; } = YouTubeFailReason.None;
    public Exception?        Error{ get; init; }

    public string ErrorMessage{
        get{
            if(Error == null){ return FailReason.ToString(); }

            StringBuilder SB = new StringBuilder();
            SB.Append(FailReason).Append(": ");

            Exception? Current = Error;
            bool First = true;

            while(Current != null){
                if(!First){ SB.Append(" → "); }
                SB.Append(Current.Message);
                First = false;
                Current = Current.InnerException;
            }

            return SB.ToString();
        }
    }

    public static YouTubeOperationResult<T> Ok(T Value){
        return new YouTubeOperationResult<T>{ Success = true, Value = Value };
    }

    public static YouTubeOperationResult<T> Fail(YouTubeFailReason Reason, Exception? Error = null){
        return new YouTubeOperationResult<T>{ Success = false, FailReason = Reason, Error = Error };
    }
}

public class YouTubeChannel{
    public string  ID{ get; init; } = "";
    public string  Title{ get; init; } = "";
    public string? Description{ get; init; }
    public long?   SubscriberCount{ get; init; }
    public long?   VideoCount{ get; init; }
    public long?   ViewCount{ get; init; }
    public string  UploadsPlaylistID{ get; init; } = "";
}

public class YouTubeVideo{
    public string          ID{ get; init; } = "";
    public string          Title{ get; init; } = "";
    public string?         Description{ get; init; }
    public YouTubePrivacy? Privacy{ get; init; }
    public DateTime?       PublishedAt{ get; init; }
}

public class YouTube{
    private readonly YouTubeService __Service;

    public YouTubeChannel MyChannel{ get; private set; } = null!;

    // ----------------------------------------------------------------------

    public const int DefaultChunkSizeBytes = ResumableUpload.MinimumChunkSize * 40;

    // ----------------------------------------------------------------------

    public YouTube(string ClientSecret, string UserLoginFolderPath, string ApplicationName = "WoowzLib.YouTube"){
        try{
            if(!Directory.Exists(UserLoginFolderPath)){
                throw new ExceptionWL($"Папка [{UserLoginFolderPath}] для хранения данных авторизации не существует! Создайте эту папке перед запуском.");
            }

            UserCredential Credential;

            using(MemoryStream MS = new MemoryStream(Encoding.UTF8.GetBytes(ClientSecret))){
                Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(MS).Secrets,
                    [YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.YoutubeReadonly],
                    "user",
                    CancellationToken.None,
                    new FileDataStore(UserLoginFolderPath, true)
                ).GetAwaiter().GetResult();
            }

            __Service = new YouTubeService(new BaseClientService.Initializer{
                HttpClientInitializer = Credential,
                ApplicationName       = ApplicationName
            });

            __Service.HttpClient.Timeout = Timeout.InfiniteTimeSpan;

            MyChannel = __LoadMyChannel();
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при авторизации в YouTube API!", e);
        }
    }

    private YouTubeChannel __LoadMyChannel(){
        try{
            ChannelsResource.ListRequest Request = __Service.Channels.List("snippet,contentDetails,statistics");
            Request.Mine = true;

            ChannelListResponse Response = Request.Execute();

            Channel? Channel__ = Response.Items?.FirstOrDefault();
            if(Channel__ == null){ throw new ExceptionWL("Не найден ни один канал у текущего аккаунта!"); }

            string UploadsPlaylistID = Channel__.ContentDetails?.RelatedPlaylists?.Uploads
                ?? throw new ExceptionWL("У канала отсутствует информация о плейлисте загрузок (ContentDetails.RelatedPlaylists.Uploads)!");

            return new YouTubeChannel{
                ID                = Channel__.Id,
                Title             = Channel__.Snippet.Title,
                Description       = Channel__.Snippet.Description,
                SubscriberCount   = (long?)Channel__.Statistics?.SubscriberCount,
                VideoCount        = (long?)Channel__.Statistics?.VideoCount,
                ViewCount         = (long?)Channel__.Statistics?.ViewCount,
                UploadsPlaylistID = UploadsPlaylistID
            };
        }catch(Exception e){
            throw new ExceptionWL("Не удалось получить информацию о своём канале!", e);
        }
    }

    public List<YouTubeVideo> ListUploads(int MaxResults = 50){
        if(MaxResults <= 0){ return []; }

        try{
            List<YouTubeVideo> Result = [];
            string? PageToken = null;

            do{
                PlaylistItemsResource.ListRequest Request = __Service.PlaylistItems.List("snippet");
                Request.PlaylistId = MyChannel.UploadsPlaylistID;
                Request.MaxResults = System.Math.Min(50, MaxResults - Result.Count);
                Request.PageToken   = PageToken;

                PlaylistItemListResponse Response = Request.Execute();

                if(Response.Items != null){
                    foreach(PlaylistItem Item in Response.Items){
                        Result.Add(new YouTubeVideo{
                            ID          = Item.Snippet.ResourceId.VideoId,
                            Title       = Item.Snippet.Title,
                            Description = Item.Snippet.Description,
                            PublishedAt = Item.Snippet.PublishedAtDateTimeOffset?.DateTime
                        });
                    }
                }

                PageToken = Response.NextPageToken;
            }while(PageToken != null && Result.Count < MaxResults);

            return Result;
        }catch(Exception e){
            throw new ExceptionWL("Не удалось получить список загруженных видео!", e);
        }
    }

    public YouTubeOperationResult<YouTubeVideo> UploadVideo(
        string VideoPath,
        string Title = "New Video [Published by WoowzLib.YouTube (by paths)]",
        string Description = "",
        YouTubePrivacy Privacy = YouTubePrivacy.Unpublic,
        List<string>? Tags = null,
        string? ThumbnailPath = null,
        Action<long, long>? OnProgress = null,
        int? ChunkSizeBytes = null
    ){
        try{
            if(!File.Exists(VideoPath)){
                return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.NotFound,
                    new ExceptionWL($"Файл видео не найден!\nПуть: {VideoPath}"));
            }

            using FileStream FS = new FileStream(VideoPath, FileMode.Open, FileAccess.Read);

            Stream? ThumbStream = null;
            try{
                if(ThumbnailPath != null){
                    if(!File.Exists(ThumbnailPath)){
                        return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.NotFound,
                            new ExceptionWL($"Файл превью не найден!\nПуть: {ThumbnailPath}"));
                    }

                    ThumbStream = new FileStream(ThumbnailPath, FileMode.Open, FileAccess.Read);
                }

                return UploadVideo(FS, FS.Length, Title, Description, Privacy, Tags, ThumbStream, OnProgress, ChunkSizeBytes);
            }finally{
                ThumbStream?.Dispose();
            }
        }catch(Exception e){
            return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.Other,
                new ExceptionWL($"Не удалось загрузить видео [{VideoPath}]!", e));
        }
    }

    public YouTubeOperationResult<YouTubeVideo> UploadVideo(
        byte[] VideoBytes,
        string Title = "New Video [Published by WoowzLib.YouTube (by bytes)]",
        string Description = "",
        YouTubePrivacy Privacy = YouTubePrivacy.Unpublic,
        List<string>? Tags = null,
        byte[]? ThumbnailBytes = null,
        Action<long, long>? OnProgress = null,
        int? ChunkSizeBytes = null
    ){
        try{
            using MemoryStream MS = new MemoryStream(VideoBytes);

            Stream? ThumbStream = null;
            try{
                if(ThumbnailBytes != null){
                    ThumbStream = new MemoryStream(ThumbnailBytes);
                }

                return UploadVideo(MS, VideoBytes.LongLength, Title, Description, Privacy, Tags, ThumbStream, OnProgress, ChunkSizeBytes);
            }finally{
                ThumbStream?.Dispose();
            }
        }catch(Exception e){
            return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.Other,
                new ExceptionWL("Не удалось загрузить видео из массива байт!", e));
        }
    }

    public YouTubeOperationResult<YouTubeVideo> UploadVideo(
        Stream VideoStream,
        long TotalBytes,
        string Title = "New Video [Published by WoowzLib.YouTube]",
        string Description = "",
        YouTubePrivacy Privacy = YouTubePrivacy.Unpublic,
        List<string>? Tags = null,
        Stream? ThumbnailStream = null,
        Action<long, long>? OnProgress = null,
        int? ChunkSizeBytes = null
    ){
        try{
            Video VideoMeta = new Video{
                Snippet = new VideoSnippet{
                    Title       = Title,
                    Description = Description,
                    Tags        = Tags ?? []
                },
                Status = new VideoStatus{
                    PrivacyStatus = Privacy switch{
                        YouTubePrivacy.Public   => "public",
                        YouTubePrivacy.Link     => "unlisted",
                        YouTubePrivacy.Unpublic => "private",
                        var _                   => "private"
                    }
                }
            };

            VideosResource.InsertMediaUpload Request = __Service.Videos.Insert(VideoMeta, "snippet,status", VideoStream, "video/*");

            Request.ChunkSize = __NormalizeChunkSize(ChunkSizeBytes);

            Request.ProgressChanged += Progress => {
                if(Progress.Status == UploadStatus.Uploading){
                    OnProgress?.Invoke(Progress.BytesSent, TotalBytes);
                }
            };

            IUploadProgress UploadResult;

            try{
                UploadResult = Request.UploadAsync().GetAwaiter().GetResult();
            }catch(TaskCanceledException e){
                return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.Cancelled, e);
            }catch(OperationCanceledException e){
                return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.Cancelled, e);
            }catch(HttpRequestException e){
                return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.NetworkError, e);
            }catch(IOException e){
                return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.NetworkError, e);
            }catch(GoogleApiException e){
                return YouTubeOperationResult<YouTubeVideo>.Fail(__ClassifyError(e), e);
            }

            if(UploadResult.Status != UploadStatus.Completed){
                if(UploadResult.Exception is GoogleApiException UploadGAE){
                    return YouTubeOperationResult<YouTubeVideo>.Fail(__ClassifyError(UploadGAE), UploadGAE);
                }

                Exception EffectiveError = UploadResult.Exception
                    ?? new ExceptionWL($"Загрузка видео завершилась со статусом [{UploadResult.Status}] без детального исключения от Google API.");

                return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.Other, EffectiveError);
            }

            Video? Uploaded = Request.ResponseBody;

            if(Uploaded == null){
                return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.Other,
                    new ExceptionWL("Загрузка сообщила об успехе, но тело ответа от YouTube API пустое!"));
            }

            OnProgress?.Invoke(TotalBytes, TotalBytes);

            if(ThumbnailStream != null){
                try{
                    __Service.Thumbnails.Set(Uploaded.Id, ThumbnailStream, "image/jpeg").Upload();
                }catch(Exception e){
                    WL.Logger.Warn($"Видео [{Uploaded.Id}] загружено успешно, но не удалось установить превью!", e);
                }
            }

            return YouTubeOperationResult<YouTubeVideo>.Ok(new YouTubeVideo{
                ID          = Uploaded.Id,
                Title       = Uploaded.Snippet.Title,
                Description = Uploaded.Snippet.Description,
                Privacy     = Privacy
            });
        }catch(GoogleApiException e){
            return YouTubeOperationResult<YouTubeVideo>.Fail(__ClassifyError(e), e);
        }

        catch(Exception e){
            return YouTubeOperationResult<YouTubeVideo>.Fail(YouTubeFailReason.Other, e);
        }
    }

    // ----------------------------------------------------------------------

    private static int __NormalizeChunkSize(int? RequestedChunkSizeBytes){
        int Requested = RequestedChunkSizeBytes ?? DefaultChunkSizeBytes;

        if(Requested < ResumableUpload.MinimumChunkSize){
            return ResumableUpload.MinimumChunkSize;
        }

        return (Requested / ResumableUpload.MinimumChunkSize) * ResumableUpload.MinimumChunkSize;
    }

    // ----------------------------------------------------------------------

    private static YouTubeFailReason __ClassifyError(GoogleApiException e){
        string? Reason = e.Error?.Errors?.FirstOrDefault()?.Reason;

        YouTubeFailReason ByReason = Reason switch{
            "quotaExceeded"           => YouTubeFailReason.QuotaExceeded,
            "dailyLimitExceeded"      => YouTubeFailReason.QuotaExceeded,
            "rateLimitExceeded"       => YouTubeFailReason.QuotaExceeded,
            "userRateLimitExceeded"   => YouTubeFailReason.QuotaExceeded,
            "uploadLimitExceeded"     => YouTubeFailReason.UploadLimitExceeded,
            "forbidden"               => YouTubeFailReason.Forbidden,
            "insufficientPermissions" => YouTubeFailReason.Forbidden,
            "videoNotFound"           => YouTubeFailReason.NotFound,
            "notFound"                => YouTubeFailReason.NotFound,
            "authError"               => YouTubeFailReason.AuthenticationError,
            "unauthorized"            => YouTubeFailReason.AuthenticationError,
            var _                     => YouTubeFailReason.Other
        };

        if(ByReason != YouTubeFailReason.Other){ return ByReason; }

        return (e.Error?.Code) switch{
            401 => YouTubeFailReason.AuthenticationError,
            403 => YouTubeFailReason.Forbidden,
            404 => YouTubeFailReason.NotFound,
            429 => YouTubeFailReason.QuotaExceeded,
            var _ => YouTubeFailReason.Other
        };
    }
}