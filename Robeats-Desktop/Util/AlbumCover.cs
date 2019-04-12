namespace Robeats_Desktop.DataTypes
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class AlbumJson
    {
        [JsonProperty("album")]
        public Album Album { get; set; }
    }

    public partial class Album
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("mbid")]
        public Guid Mbid { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("image")]
        public Image[] Image { get; set; }

        [JsonProperty("listeners")]
        public long Listeners { get; set; }

        [JsonProperty("playcount")]
        public long Playcount { get; set; }

        [JsonProperty("tracks")]
        public Tracks Tracks { get; set; }

        [JsonProperty("tags")]
        public Tags Tags { get; set; }

        [JsonProperty("wiki")]
        public Wiki Wiki { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("#text")]
        public Uri Text { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }

    public partial class Tags
    {
        [JsonProperty("tag")]
        public object[] Tag { get; set; }
    }

    public partial class Tracks
    {
        [JsonProperty("track")]
        public Track[] Track { get; set; }
    }

    public partial class Track
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("@attr")]
        public Attr Attr { get; set; }

        [JsonProperty("streamable")]
        public Streamable Streamable { get; set; }

        [JsonProperty("artist")]
        public Artist Artist { get; set; }
    }

    public partial class Artist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mbid")]
        public Guid Mbid { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class Attr
    {
        [JsonProperty("rank")]
        public long Rank { get; set; }
    }

    public partial class Streamable
    {
        [JsonProperty("#text")]
        public long Text { get; set; }

        [JsonProperty("fulltrack")]
        public long Fulltrack { get; set; }
    }

    public partial class Wiki
    {
        [JsonProperty("published")]
        public string Published { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}