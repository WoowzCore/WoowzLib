using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace WLO;

public enum YouTubeFailReason{
    None,
    QuotaExceeded,
    UploadLimitExceeded,
    Forbidden,
    NotFound,
    Other
}

public class YouTubeOperationResult<T>{
    public bool Success{ get; init; }
    public T? Value{ get; init; }
    public YouTubeFailReason FailReason{ get; init; } = YouTubeFailReason.None;
    public Exception? Error{ get; init; }
}

public class YouTubeChannel{
    public string ID{ get; init; } = "";
    public string Title{ get; init; } = "";
    public string? Description{ get; init; }
    public long? SubscriberCount{ get; init; }
    public long? VideoCount{ get; init; }
    public long? ViewCount{ get; init; }
    public string UploadsPlaylistID{ get; init; } = "";
}

public class YouTubeVideo{
    public string ID{ get; init; } = "";
    public string Title{ get; init; } = "";
    public string? Description{ get; init; }
    public string? PrivacyStatus{ get; init; }
    public DateTime? PublishedAt{ get; init; }
}

public class YouTube{
    private readonly YouTubeService __Service;

    public YouTubeChannel MyChannel{ get; private set; } = null!;

    public YouTube(string ClientSecret, string UserLoginFolderPath, string ApplicationName = "WoowzLib.YouTube"){
        try{
            if(!Directory.Exists(UserLoginFolderPath)){
                throw new Exception($"Папка [{UserLoginFolderPath}] для хранения данных авторизации не существует! Создайте эту папке перед запуском.");
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

            return new YouTubeChannel{
                ID                = Channel__.Id,
                Title             = Channel__.Snippet.Title,
                Description       = Channel__.Snippet.Description,
                SubscriberCount   = (long?)Channel__.Statistics?.SubscriberCount,
                VideoCount        = (long?)Channel__.Statistics?.VideoCount,
                ViewCount         = (long?)Channel__.Statistics?.ViewCount,
                UploadsPlaylistID = Channel__.ContentDetails.RelatedPlaylists.Uploads
            };
        }catch(Exception e){
            throw new ExceptionWL("Не удалось получить информацию о своём канале!", e);
        }
    }

    public List<YouTubeVideo> ListUploads(int MaxResults = 50){
        try{
            List<YouTubeVideo> Result = [];
            string? PageToken = null;

            do{
                PlaylistItemsResource.ListRequest Request = __Service.PlaylistItems.List("snippet");
                Request.PlaylistId = MyChannel.UploadsPlaylistID;
                Request.MaxResults = System.Math.Min(50, MaxResults - Result.Count);
                Request.PageToken = PageToken;

                PlaylistItemListResponse Response = Request.Execute();

                foreach(PlaylistItem? Item in Response.Items){
                    Result.Add(new YouTubeVideo{
                        ID          = Item.Snippet.ResourceId.VideoId,
                        Title       = Item.Snippet.Title,
                        Description = Item.Snippet.Description,
                        PublishedAt = Item.Snippet.PublishedAtDateTimeOffset?.DateTime
                    });
                }

                PageToken = Response.NextPageToken;
            }while(PageToken != null && Result.Count < MaxResults);

            return Result;
        }catch(Exception e){
            throw new ExceptionWL("Не удалось получить список загруженных видео!", e);
        }
    }
}