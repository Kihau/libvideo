using System;
using System.Linq;
using VideoLibrary;

var downloader = new YouTube();
var videos = await downloader.GetAllVideosAsync("https://www.youtube.com/watch?v=pqrUQrAcfo4");
var video = videos.Where(x => x.Resolution == -1);
//File.WriteAllBytes("/home/kihau/" + video.FullName, video.GetBytes());

// 140, 249, 250, 251
