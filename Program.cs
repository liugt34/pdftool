using ImageMagick;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace pdftool
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            // 定义 Root command
            var command = new RootCommand("This is a tool for pdf. has functions like merge images to pdf,import pdf page to images");

            #region Merge Images To PDF

            var mergeCommand = new Command("merge", "Merge Images to pdf,support jpg and png only");

            var dirOption = new Option<string>("--dir", "The images directory path");
            dirOption.IsRequired = true;
            dirOption.AddAlias("-d"); 
            dirOption.AddValidator((option) =>
            {
                var dir = option.GetValueForOption<string>(dirOption);
                
                if (!Directory.Exists(dir))
                {
                    option.ErrorMessage = $"The images directory '{dir}' doesn't exist";
                }
            });
            mergeCommand.AddOption(dirOption);

            var nameOption = new Option<string>("--name", "The pdf name");
            nameOption.IsRequired = true;
            nameOption.AddAlias("-n");
            mergeCommand.AddOption(nameOption);

            mergeCommand.SetHandler(
                (dir, name) =>  
                {
                    MergeImagesInDirectory(dir, name);
                },
                dirOption, nameOption);

            command.AddCommand(mergeCommand);

            #endregion

            #region Export PDF AS Images

            var exportCommand = new Command("export", "Export PDF to images");

            var srcOption = new Option<string>("--source", "The PDF file path");
            srcOption.IsRequired = true;
            srcOption.AddAlias("-s");
            srcOption.AddValidator((option) =>
            {
                var path = option.GetValueForOption<string>(srcOption);

                if (!File.Exists(path))
                {
                    option.ErrorMessage = $"The PDF file '{path}' doesn't exist";
                }
            });
            exportCommand.AddOption(srcOption);

            var exportOption = new Option<string>("--dir", "The images directory path");
            exportOption.IsRequired = true;
            exportOption.AddAlias("-d");
            exportOption.AddValidator((option) =>
            {
                var dir = option.GetValueForOption<string>(exportOption);

                if (!Directory.Exists(dir))
                {
                    option.ErrorMessage = $"The images directory '{dir}' doesn't exist";
                }
            });
            exportCommand.AddOption(exportOption);

            var startOption = new Option<string>("--start", "The PDF export page start number");
            startOption.AddValidator((option) =>
            {
                var value = option.GetValueOrDefault<string>();

                if (int.TryParse(value, out var pageNo))
                {
                    if (pageNo < 1)
                    {
                        option.ErrorMessage = "page start must be greater than 0";
                    }
                }
                else
                {
                    option.ErrorMessage = "invalid start page";
                }
            });
            exportCommand.AddOption(startOption);

            var numberOption = new Option<string>("--number", "The number of pages will be exported");
            numberOption.AddValidator((option) =>
            {
                var value = option.GetValueOrDefault<string>();

                if (int.TryParse(value, out var pageNo))
                {
                    if (pageNo < 1)
                    {
                        option.ErrorMessage = "page number must be greater than 0";
                    }
                }
                else
                {
                    option.ErrorMessage = "invalid page number";
                }
            });
            exportCommand.AddOption(numberOption);

            exportCommand.SetHandler(
                (source, dir, start, number) =>
                {
                    ExportPdfToImages(source, dir, start, number);
                },
                srcOption, exportOption, startOption, numberOption);

            command.AddCommand(exportCommand);

            #endregion

            return await command.InvokeAsync(args);
        }

        static void MergeImagesInDirectory(string dir, string name)
        {
#if DEBUG
            Console.WriteLine($"dir={dir}, name={name}");
#endif
            using var images = new MagickImageCollection();

            var settings = new MagickReadSettings();
            //A4 size
            //settings.Width = 595;
            //settings.Height = 842;
            settings.Page = new MagickGeometryFactory().CreateFromPageSize("a4");

            var files = Directory.EnumerateFiles(dir)
                                 .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png"))
                                 .ToArray();

            foreach (var file in files)
            {
                var image = new MagickImage(file, settings);
				image.Density = new Density(72);

                if (image.Width > settings.Page.Width || image.Height > settings.Page.Height)
                {
                    // 计算图片应该被缩放到多大才能适应页面宽度并居中
                    double scaleX = settings.Page.Width / (double)image.Width;
                    double scaleY = settings.Page.Height / (double)image.Height;
                    double scale = scaleX < scaleY ? scaleX : scaleY;
                    int newX = (int)(image.Width * scale);
                    int newY = (int)(image.Height * scale);

                    // 设置图片的缩放和定位
                    image.Resize(newX, newY);
                }

                //图片居中显示
                image.Extent(settings.Page, Gravity.Center);

                images.Add(image);
            }

            // Create pdf file with two pages
            images.Write(Path.Combine(dir, $"{name}.pdf"));
        }

        static void ExportPdfToImages(string source, string dir, string start, string number)
        {
            Console.WriteLine($"src={source}, dir={dir}, start={start}, end={number};");

            // Settings the density to 300 dpi will create an image with a better quality
            var settings = new MagickReadSettings
            {
                Density = new Density(300, 300)
            };

            if (int.TryParse(start, out var s))
            {
                settings.FrameIndex = s - 1;
            }
            if (int.TryParse(number, out var n))
            {
                settings.FrameCount = n;
            }

            using var images = new MagickImageCollection();

            // Add all the pages of the pdf file to the collection
            images.Read(source, settings);

            var page = string.IsNullOrEmpty(start) ? 1 : s;
            foreach (var image in images)
            {
                // Write page to file that contains the page number
                image.Write(Path.Combine(dir, page + ".jpg"));
                page++;
            }
        }
    }
}
