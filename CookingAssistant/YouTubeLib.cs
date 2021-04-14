using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;

/// <summary>
/// Namespace housing classes acting as a wrapper for YouTube Data Api.
/// </summary>
namespace YouTubeLib
{
    /// <summary>
    /// Handle used to connect to YouTube and manage communication between the service and the application
    /// </summary>
    public class YouTubeHandle
    {
        private readonly YouTubeService service;
        public YouTubeHandle(string apiKey)
        {
            service = new YouTubeService(new YouTubeService.Initializer() { ApiKey = apiKey });
        }
        /// <summary>
        /// Searches for videos related to a given term and return a list of found results
        /// </summary>
        /// <param name="searchTerm">Term to search</param>
        /// <param name="maxResults">Maximum number of results</param>
        /// <returns>A list of videos found related to the search term</returns>
        public async Task<List<YouTubeUtils.Video>> SearchVideos(string searchTerm, int maxResults)
        {
            var searchListRequest = service.Search.List("snippet");
            searchListRequest.Q = searchTerm;
            searchListRequest.MaxResults = maxResults;
            searchListRequest.Type = "video";
            // searchListRequest.VideoEmbeddable = SearchResource.ListRequest.VideoEmbeddableEnum.True__;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            var results = new List<YouTubeUtils.Video>();
            foreach (var result in searchListResponse.Items)
            {
                string id = Convert.ToString(result.Id.VideoId);
                string title = result.Snippet.Title;
                string channel = result.Snippet.ChannelTitle;
                string description = result.Snippet.Description;

                string thumbnailUrl = Convert.ToString(result.Snippet.Thumbnails.High.Url);
                int thumbnailWidth = Convert.ToInt32(result.Snippet.Thumbnails.High.Width);
                int thumbnailHeight = Convert.ToInt32(result.Snippet.Thumbnails.High.Height);

                results.Add(new YouTubeUtils.Video(id, title, channel, description, new YouTubeUtils.Thumbnail(thumbnailUrl, thumbnailWidth, thumbnailHeight)));
            }
            return results;
        }
        /// <summary>
        /// Retrieves videos using saved ids
        /// </summary>
        /// <param name="videoIds">List of saved YouTube video ids</param>
        /// <returns>A list of retrived videos</returns>
        public async Task<List<YouTubeUtils.Video>> GetVideosFromIds(List<string> videoIds)
        {
            var videoRequest = service.Videos.List("snippet");
            videoRequest.Id = videoIds;
            var videoResponse = await videoRequest.ExecuteAsync();
            var results = new List<YouTubeUtils.Video>();
            foreach (var result in videoResponse.Items)
            {
                string id = Convert.ToString(result.Id);
                string title = result.Snippet.Title;
                string channel = result.Snippet.ChannelTitle;
                string description = result.Snippet.Description;

                string thumbnailUrl = Convert.ToString(result.Snippet.Thumbnails.High.Url);
                int thumbnailWidth = Convert.ToInt32(result.Snippet.Thumbnails.High.Width);
                int thumbnailHeight = Convert.ToInt32(result.Snippet.Thumbnails.High.Height);

                results.Add(new YouTubeUtils.Video(id, title, channel, description, new YouTubeUtils.Thumbnail(thumbnailUrl, thumbnailWidth, thumbnailHeight)));
            }
            return results;
        }
        /*
        public async Task<bool> SynchronizeVideoList(List<Video> videos)
        {
            var videosRequest = service.Videos.List("snippet");
            foreach (var video in videos)
            {
                videosRequest.
            }
            var videosResponse = await videosRequest.ExecuteAsync();
            return true;
        }
        */
    }
    ///
    public static class YouTubeUtils
    {
        public class Thumbnail
        {
            public string Url { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public Thumbnail(string url, int width, int height)
            {
                this.Url = url;
                this.Width = width;
                this.Height = height;
            }
            public Thumbnail()
            {
                this.Url = null;
                this.Width = 0;
                this.Height = 0;
            }
        }
        public class Video
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Channel { get; set; }
            public string Description { get; set; }
            public Thumbnail Thumbnail { get; set; }
            public Video(string id, string title, string channel, string description, Thumbnail thumbnail)
            {
                this.Id = id;
                this.Title = title;
                this.Channel = channel;
                this.Description = description;
                this.Thumbnail = thumbnail;
            }
            public override string ToString()
            {
                return this.Title;
            }
        }
        public static Thumbnail RetrieveThumbnailData(SearchResult result)
        {
            string thumbnailUrl = Convert.ToString(result.Snippet.Thumbnails.High.Url);
            int thumbnailWidth = Convert.ToInt32(result.Snippet.Thumbnails.High.Width);
            int thumbnailHeight = Convert.ToInt32(result.Snippet.Thumbnails.High.Height);
            return new Thumbnail(thumbnailUrl, thumbnailWidth, thumbnailHeight);
            // tu będą wyjątki i sposób pobrania najlepszej możliwej rodzielczości miniaturki
        }
        public static Video RetrieveVideoData(SearchResult result)
        {
            string id = Convert.ToString(result.Id.VideoId);
            string title = result.Snippet.Title;
            string channel = result.Snippet.ChannelTitle;
            string description = result.Snippet.Description;
            Thumbnail thumbnail = RetrieveThumbnailData(result);
            return new Video(id, title, channel, description, thumbnail);
        }
    }
}