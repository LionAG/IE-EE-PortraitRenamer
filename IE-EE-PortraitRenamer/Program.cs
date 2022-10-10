using CommandLine;

namespace IE_EE_PortraitRenamer
{
    public class Arguments
    {
        [Value(0, MetaName = "portrait_folder_path", HelpText = "Path to the folder containing bitmap files to process.")]
        public string PortraitFolderPath { get; init; }

        [Option('p', "prefix", Required = false, HelpText = "Use prefix for renaming.", Default = "")]
        public string RenamePrefix { get; init; }

        [Option('r', "--random-rename", Required = false, HelpText = "Use this switch to rename all files to random names first.")]
        public bool RandomRename { get; init; }
    }

    public static class Program
    {
        private static List<string> GetBitmaps(string path) => Directory.EnumerateFiles(path)
                                                                        .Where(f => Path.GetExtension(f).Equals(".bmp", StringComparison.OrdinalIgnoreCase))
                                                                        .ToList();

        private static void RandomBitmapRenamer(string path) => GetBitmaps(path).ForEach(b => File.Move(b, Path.Combine(Path.GetDirectoryName(b),
                                                                                                                        Path.ChangeExtension(Path.GetRandomFileName(), "bmp"))));

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Arguments>(args).WithParsed(a =>
            {
                var path = a.PortraitFolderPath;
                var prefix = a.RenamePrefix;

                if (!Directory.Exists(path))
                {
                    throw new ArgumentException($"{path} is not a valid directory!");
                }

                if (a.RandomRename)
                {
                    RandomBitmapRenamer(path);
                    Console.WriteLine("Processing finished!");

                    return;
                }

                if (prefix.Length > 4)
                {
                    throw new ArgumentException($"{prefix} is an invalid prefix. {nameof(prefix)} cannot be longer than 4 characters!");
                }

                // Enumerate bitmap files.

                var bitmaps = GetBitmaps(path);

                // Attempting to rename more files this way would result in a file name longer than 8 chars
                // this is not supported by the engine.
                if (bitmaps.Count > 9999)
                {
                    throw new InvalidOperationException($"Cannot rename more than 9999 elements! {path} contains {bitmaps.Count}");
                }

                var count = 0;

                bitmaps.ForEach((filePath) =>
                {
                    var extension = Path.GetExtension(filePath);
                    var directory = Path.GetDirectoryName(filePath);
                    var newPath = Path.Combine(directory, $"{prefix}{count}{extension}");

                    if (File.Exists(newPath))
                    {
                        throw new Exception($"File {newPath} already exists. You can fix this by running the program with --random-rename switch first.");
                    }

                    File.Move(filePath, newPath);

                    count++;
                });

                Console.WriteLine($"Done, processed {count} files!");

            });
        }
    }
}