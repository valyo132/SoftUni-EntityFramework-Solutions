using MusicHub.Data;
using MusicHub.Initializer;
using System.Globalization;
using System.Text;

namespace MusicHub
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context = new MusicHubDbContext();
            DbInitializer.ResetDatabase(context);

            var result = ExportSongsAboveDuration(context, 4);

            Console.WriteLine(result);
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            StringBuilder sb = new StringBuilder();

            var songs = context.Songs
                .AsEnumerable()
                .Where(s => s.Duration.TotalSeconds > duration)
                .Select(s => new
                {
                    Name = s.Name,
                    PerformerFullNames = s.SongPerformers.Select(sp => sp.Performer.FirstName + " " + sp.Performer.LastName)
                        .OrderBy(sp => sp)
                        .ToArray(),
                    WriterName = s.Writer.Name,
                    AlbumProducer = s.Album.Producer.Name,
                    Duration = s.Duration.ToString("c")
                }).OrderBy(s => s.Name)
                .ThenBy(s => s.WriterName)
                .ToList();

            int index = 0;
            foreach (var song in songs)
            {
                sb.AppendLine($"-Song #{++index}");
                sb.AppendLine($"---SongName: {song.Name}");
                sb.AppendLine($"---Writer: {song.WriterName}");

                if (song.PerformerFullNames.Any())
                {
                    foreach (var performer in song.PerformerFullNames)
                    {
                        sb.AppendLine($"---Performer: {performer}");
                    } 
                }

                sb.AppendLine($"---AlbumProducer: {song.AlbumProducer}");
                sb.AppendLine($"---Duration: {song.Duration}");
            }

            return sb.ToString().TrimEnd();
        }
        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            StringBuilder sb = new StringBuilder();

            var albums = context.Albums
                .Where(a => a.ProducerId == producerId)
                .Select(a => new
                {
                    Name = a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                    ProducerName = a.Producer.Name,
                    Price = a.Price,
                    AlbumSongs = a.Songs
                        .OrderByDescending(s => s.Name).ToList(),
                    SongWriters = a.Songs.Select(s => s.Writer)
                }).ToList();
                

            foreach (var album in albums.OrderByDescending(a => a.Price))
            {
                sb.AppendLine($"-AlbumName: {album.Name}");
                sb.AppendLine($"-ReleaseDate: {album.ReleaseDate}");
                sb.AppendLine($"-ProducerName: {album.ProducerName}");

                sb.AppendLine("-Songs:");
                int index = 1;

                foreach (var song in album.AlbumSongs)
                {
                    sb.AppendLine($"---#{index}");
                    sb.AppendLine($"---SongName: {song.Name}");
                    sb.AppendLine($"---Price: {song.Price:f2}");
                    sb.AppendLine($"---Writer: {song.Writer.Name}");

                    index++;
                }

                sb.AppendLine($"-AlbumPrice: {album.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}